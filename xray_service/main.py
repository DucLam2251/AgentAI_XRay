import base64
import csv
import io
import json
import os
import re
import shutil
import subprocess
import sys
from datetime import datetime
from pathlib import Path

from fastapi import BackgroundTasks, FastAPI, File, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
from PIL import Image
from ultralytics import YOLO

app = FastAPI(title="Bone Fracture Detection Service")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

MODELS_DIR = Path(__file__).parent / "models"
MODELS_DIR.mkdir(exist_ok=True)
ACTIVE_MODEL_FILE = MODELS_DIR / "active_model.txt"

EXCLUDED_CLASSES = {"text"}

_model: YOLO | None = None
_train_state: dict = {
    "status": "idle",
    "log": [],
    "proc": None,
    "device": "auto",
    "current_epoch": 0,
    "total_epochs": 0,
    "progress_pct": 0,
}


# ─── Model helpers ───────────────────────────────────────────────────────────

def _read_active_name() -> str:
    if ACTIVE_MODEL_FILE.exists():
        name = ACTIVE_MODEL_FILE.read_text().strip()
        if (MODELS_DIR / name).exists():
            return name
    # fallback: first .pt in models/
    pts = sorted(MODELS_DIR.glob("*.pt"))
    return pts[0].name if pts else "yolov8n.pt"


def _active_model_path() -> str:
    name = _read_active_name()
    p = MODELS_DIR / name
    return str(p) if p.exists() else "yolov8n.pt"


def _load_model() -> YOLO:
    global _model
    path = _active_model_path()
    _model = YOLO(path)
    print(f"Loaded model: {path}")
    return _model


def get_model() -> YOLO:
    if _model is None:
        return _load_model()
    return _model


# ─── Core detection ──────────────────────────────────────────────────────────

@app.get("/health")
def health():
    return {"status": "ok", "model": _read_active_name()}


@app.post("/model/reload")
def reload_model():
    global _model
    _model = None
    try:
        get_model()
        return {"success": True, "model": _read_active_name()}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/detect")
async def detect(file: UploadFile = File(...)):
    contents = await file.read()
    try:
        image = Image.open(io.BytesIO(contents)).convert("RGB")
    except Exception:
        raise HTTPException(status_code=400, detail="Invalid image file")

    model = get_model()
    results = model(image, conf=0.25, verbose=False)
    result = results[0]

    detections = []
    for box in result.boxes:
        cls_id = int(box.cls[0])
        label = model.names[cls_id]
        if label in EXCLUDED_CLASSES:
            continue
        conf = float(box.conf[0])
        xyxy = box.xyxy[0].tolist()
        detections.append({
            "label": label,
            "confidence": round(conf, 3),
            "bbox": [round(x) for x in xyxy],
        })

    import cv2
    img = result.orig_img.copy()  # BGR numpy array
    for i, box in enumerate(result.boxes):
        if model.names[int(box.cls[0])] in EXCLUDED_CLASSES:
            continue
        cls_id = int(box.cls[0])
        label = model.names[cls_id]
        conf = float(box.conf[0])
        x1, y1, x2, y2 = [int(v) for v in box.xyxy[0].tolist()]
        color = (0, 200, 100)
        cv2.rectangle(img, (x1, y1), (x2, y2), color, 2)
        text = f"{label} {conf:.2f}"
        (tw, th), _ = cv2.getTextSize(text, cv2.FONT_HERSHEY_SIMPLEX, 0.55, 1)
        cv2.rectangle(img, (x1, y1 - th - 6), (x1 + tw + 4, y1), color, -1)
        cv2.putText(img, text, (x1 + 2, y1 - 4), cv2.FONT_HERSHEY_SIMPLEX, 0.55, (0, 0, 0), 1)

    annotated_rgb = Image.fromarray(img[..., ::-1])
    buf = io.BytesIO()
    annotated_rgb.save(buf, format="JPEG", quality=90)
    annotated_b64 = base64.b64encode(buf.getvalue()).decode()

    return {
        "detections": detections,
        "annotated_image_base64": annotated_b64,
        "model_used": _read_active_name(),
    }


# ─── Model management ────────────────────────────────────────────────────────

@app.get("/models/list")
def list_models():
    active = _read_active_name()
    trained, origin = [], None
    for f in sorted(MODELS_DIR.glob("*.pt"), key=lambda p: p.stat().st_mtime, reverse=True):
        item = {
            "name": f.name,
            "size_mb": round(f.stat().st_size / 1024 / 1024, 1),
            "active": f.name == active,
            "is_origin": f.name == "bone_xray_base.pt",
            "modified": datetime.fromtimestamp(f.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
        }
        if item["is_origin"]:
            origin = item
        else:
            trained.append(item)
    models = trained + ([origin] if origin else [])
    return {"models": models}


@app.post("/models/activate/{filename}")
def activate_model(filename: str):
    global _model
    target = MODELS_DIR / filename
    if not target.exists():
        raise HTTPException(status_code=404, detail=f"{filename} not found")
    ACTIVE_MODEL_FILE.write_text(filename)
    _model = None
    get_model()
    return {"success": True, "active": filename}


def _read_final_metrics(run_dir: Path) -> dict:
    csv_path = run_dir / "results.csv"
    if not csv_path.exists():
        return {}
    with open(csv_path, newline="") as f:
        rows = list(csv.DictReader(f))
    if not rows:
        return {}
    last = {k.strip(): v.strip() for k, v in rows[-1].items()}
    keys = {
        "mAP50": "metrics/mAP50(B)",
        "mAP50_95": "metrics/mAP50-95(B)",
        "precision": "metrics/precision(B)",
        "recall": "metrics/recall(B)",
        "epochs_done": "epoch",
    }
    return {label: last.get(col, "") for label, col in keys.items()}


def _img_to_b64(path: Path) -> str:
    if not path.exists():
        return ""
    return base64.b64encode(path.read_bytes()).decode()


@app.get("/models/{filename}/info")
def model_info(filename: str):
    if filename == "bone_xray_base.pt":
        raise HTTPException(status_code=400, detail="Model gốc không có thông tin training")
    meta_path = MODELS_DIR / filename.replace(".pt", ".json")
    if not meta_path.exists():
        raise HTTPException(status_code=404, detail="Không tìm thấy metadata")
    meta = json.loads(meta_path.read_text())
    run_dir = Path(meta.get("run_dir", ""))
    return {
        **meta,
        "images": {
            "results": _img_to_b64(run_dir / "results.png"),
            "confusion_matrix": _img_to_b64(run_dir / "confusion_matrix.png"),
            "val_pred": _img_to_b64(run_dir / "val_batch0_pred.jpg"),
        },
    }


@app.delete("/models/{filename}")
def delete_model(filename: str):
    if filename == "bone_xray_base.pt":
        raise HTTPException(status_code=400, detail="Không thể xóa model gốc")
    if filename == _read_active_name():
        raise HTTPException(status_code=400, detail="Không thể xóa model đang active")
    target = MODELS_DIR / filename
    if not target.exists():
        raise HTTPException(status_code=404, detail=f"{filename} not found")
    target.unlink()
    meta = MODELS_DIR / filename.replace(".pt", ".json")
    if meta.exists():
        meta.unlink()
    return {"success": True}


# ─── Training ────────────────────────────────────────────────────────────────

def _run_training(data: str, epochs: int, base_model: str, imgsz: int, batch: int, device: str):
    global _train_state
    _train_state["log"] = []
    _train_state["status"] = "running"
    _train_state["new_model"] = None
    _train_state["device"] = device
    _train_state["current_epoch"] = 0
    _train_state["total_epochs"] = epochs
    _train_state["progress_pct"] = 0

    cmd = [
        sys.executable, str(Path(__file__).parent / "train.py"),
        "--data", data,
        "--epochs", str(epochs),
        "--model", base_model,
        "--imgsz", str(imgsz),
        "--batch", str(batch),
        "--device", device,
    ]

    _train_state["log"].append(f"Using requested device: {device}")

    proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, bufsize=1)
    _train_state["proc"] = proc

    for line in proc.stdout:
        line = line.rstrip()
        _train_state["log"].append(line)
        m = re.match(r"^\s*(\d+)\s*/\s*(\d+)\b", line)
        if m:
            cur = int(m.group(1))
            total = int(m.group(2))
            if total > 0:
                _train_state["current_epoch"] = cur
                _train_state["total_epochs"] = total
                pct = int(cur * 100 / total)
                # Keep progress monotonic so UI does not jump backward.
                _train_state["progress_pct"] = max(_train_state.get("progress_pct", 0), min(100, pct))
        if len(_train_state["log"]) > 500:
            _train_state["log"].pop(0)

    proc.wait()
    _train_state["proc"] = None

    if proc.returncode == 0:
        runs_dir = Path(__file__).parent / "runs" / "detect"
        candidates = sorted(runs_dir.glob("*/weights/best.pt"), key=lambda p: p.stat().st_mtime, reverse=True)
        if candidates:
            ts = datetime.now().strftime("%Y%m%d_%H%M%S")
            dest_name = f"trained_{ts}.pt"
            run_dir = candidates[0].parent.parent
            shutil.copy2(candidates[0], MODELS_DIR / dest_name)
            # save training metadata alongside model
            meta = {
                "trained_at": ts,
                "run_dir": str(run_dir),
                "config": {
                    "data": data,
                    "epochs": epochs,
                    "base_model": base_model,
                    "imgsz": imgsz,
                    "batch": batch,
                    "device": device,
                },
                "metrics": _read_final_metrics(run_dir),
            }
            (MODELS_DIR / dest_name.replace(".pt", ".json")).write_text(json.dumps(meta, indent=2))
            _train_state["new_model"] = dest_name
            _train_state["log"].append(f"✅ Training complete. Model saved: {dest_name}")
        _train_state["progress_pct"] = 100
        _train_state["current_epoch"] = _train_state.get("total_epochs", epochs)
        _train_state["status"] = "done"
    else:
        _train_state["status"] = "error"
        _train_state["log"].append(f"❌ Training failed (exit {proc.returncode})")


@app.post("/train/start")
def start_training(
    background_tasks: BackgroundTasks,
    data: str = "dataset/data.yaml",
    epochs: int = 100,
    base_model: str = "yolov8s.pt",
    imgsz: int = 640,
    batch: int = 16,
    device: str = "auto",
):
    if _train_state["status"] == "running":
        raise HTTPException(status_code=409, detail="Training already in progress")
    background_tasks.add_task(_run_training, data, epochs, base_model, imgsz, batch, device)
    return {"success": True}


@app.post("/train/stop")
def stop_training():
    proc = _train_state.get("proc")
    if proc and proc.poll() is None:
        proc.terminate()
        _train_state["status"] = "stopped"
        _train_state["log"].append("⚠️ Training stopped by user.")
        return {"success": True}
    return {"success": False, "message": "No training in progress"}


@app.get("/train/status")
def training_status():
    return {
        "status": _train_state["status"],
        "log": _train_state["log"][-50:],
        "new_model": _train_state.get("new_model"),
        "device": _train_state.get("device", "auto"),
        "current_epoch": _train_state.get("current_epoch", 0),
        "total_epochs": _train_state.get("total_epochs", 0),
        "progress_pct": _train_state.get("progress_pct", 0),
    }


# ─── Admin UI ────────────────────────────────────────────────────────────────

@app.get("/admin", response_class=HTMLResponse)
def admin_ui():
    return Path(__file__).parent.joinpath("static", "admin.html").read_text(encoding="utf-8")
