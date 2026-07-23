import base64
import csv
import gc
import io
import json
import logging
import math
import os
import re
import shutil
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

from fastapi import BackgroundTasks, FastAPI, File, HTTPException, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import HTMLResponse
from PIL import Image
import torch
import yaml
import psutil
from ultralytics import YOLO


class _PollingAccessLogFilter(logging.Filter):
    """Hide frequent admin polling requests from Uvicorn's access log."""

    QUIET_PATHS = {
        "/train/status",
        "/health",
        "/models/list",
        "/research/status",
        "/research/train/status",
    }

    def filter(self, record: logging.LogRecord) -> bool:
        # Uvicorn access-log args are:
        # (client_addr, method, full_path, http_version, status_code).
        if len(record.args) >= 3 and record.args[2] in self.QUIET_PATHS:
            return False
        return True


logging.getLogger("uvicorn.access").addFilter(_PollingAccessLogFilter())

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
RESEARCH_MODELS_DIR = MODELS_DIR / "research"
RESEARCH_REGISTRY_FILE = RESEARCH_MODELS_DIR / "registry.json"
RESEARCH_INFERENCE_DEVICE = os.getenv("XRAY_RESEARCH_DEVICE", "cpu")

EXCLUDED_CLASSES = {"text"}

_model: YOLO | None = None
_research_models: dict[str, YOLO] = {}
_research_train_proc: subprocess.Popen | None = None
_train_state: dict = {
    "status": "idle",
    "log": [],
    "proc": None,
    "device": "auto",
    "current_epoch": 0,
    "total_epochs": 0,
    "progress_pct": 0,
    "images_done": 0,
    "images_total": 0,
    "dataset_size": 0,
    "current_batch": 0,
    "batches_per_epoch": 0,
    "elapsed_seconds": 0,
    "eta_seconds": None,
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


def _release_inference_model() -> None:
    """Release the API inference model before a separate GPU training process starts."""
    global _model
    _model = None
    gc.collect()
    if torch.cuda.is_available():
        torch.cuda.empty_cache()


def _research_registry() -> dict:
    if not RESEARCH_REGISTRY_FILE.exists():
        return {"research_only": True, "models": {}}
    try:
        return json.loads(RESEARCH_REGISTRY_FILE.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return {"research_only": True, "models": {}}


def _research_model_path(key: str) -> Path | None:
    entry = _research_registry().get("models", {}).get(key, {})
    raw_path = entry.get("path")
    if not raw_path:
        return None
    path = Path(raw_path)
    if not path.is_absolute():
        path = Path(__file__).parent / path
    return path if path.exists() else None


def _get_research_model(key: str) -> YOLO | None:
    path = _research_model_path(key)
    if path is None:
        return None
    cached = _research_models.get(key)
    if cached is None:
        cached = YOLO(str(path))
        _research_models[key] = cached
    return cached


def _classification_scores(model: YOLO, image: Image.Image) -> dict:
    result = model.predict(
        image, imgsz=320, device=RESEARCH_INFERENCE_DEVICE, verbose=False
    )[0]
    if result.probs is None:
        return {}
    values = result.probs.data.detach().cpu().tolist()
    return {
        str(model.names[index]): round(float(score), 4)
        for index, score in enumerate(values)
    }


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
    if _train_state.get("status") in {"starting", "running"}:
        raise HTTPException(
            status_code=409,
            detail="GPU is being used for training. Stop or finish training before detection.",
        )
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


@app.get("/research/status")
def research_status():
    registry = _research_registry()
    required = (
        "fracture_classifier",
        "fracture_detector",
        "abnormality_classifier",
    )
    availability = {key: _research_model_path(key) is not None for key in required}
    return {
        "research_only": True,
        "ready": all(availability.values()),
        "models": availability,
        "inference_device": RESEARCH_INFERENCE_DEVICE,
        "registry": registry,
    }


RESEARCH_TASKS = {
    "fracture-classifier": {"epochs": 30, "order": 1},
    "fracture-detector": {"epochs": 50, "order": 2},
    "abnormality-classifier": {"epochs": 15, "order": 3},
}


def _research_state_path() -> Path:
    return Path(__file__).parent / "runs" / "research" / "pipeline_status.json"


def _read_research_train_state() -> dict:
    path = _research_state_path()
    if not path.exists():
        return {"status": "idle", "task": None, "message": "Pipeline has not started"}
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except (OSError, json.JSONDecodeError):
        return {"status": "unknown", "task": None, "message": "Cannot read pipeline status"}


def _pid_is_running(pid: object) -> bool:
    try:
        process = psutil.Process(int(pid))
        return process.is_running() and process.status() != psutil.STATUS_ZOMBIE
    except (psutil.Error, TypeError, ValueError):
        return False


def _research_log_details() -> dict:
    log_path = Path(__file__).parent / "runs" / "research" / "pipeline.log"
    if not log_path.exists():
        return {"log": [], "epoch": 0, "epochs": 0, "batch": 0, "batches": 0}
    with log_path.open("rb") as stream:
        stream.seek(max(0, log_path.stat().st_size - 250_000))
        raw = stream.read()
    # Windows PowerShell may append redirected output as UTF-16 LE.
    encoding = "utf-16-le" if raw.count(b"\x00") > len(raw) // 5 else "utf-8"
    text = raw.decode("utf-8-sig" if encoding == "utf-8" else encoding, errors="replace")
    clean = re.sub(r"\x1b\[[0-9;?]*[ -/]*[@-~]", "", text)
    lines = [
        line.replace("\x00", "").strip()
        for line in re.split(r"[\r\n]+", clean)
        if line.replace("\x00", "").strip()
    ]
    def is_progress_line(line: str) -> bool:
        return "%" in line and len(re.findall(r"\d+/\d+", line)) >= 2

    def is_noisy_progress_line(line: str) -> bool:
        return is_progress_line(line) or (
            ("%" in line or "Γö" in line)
            and ("it/s" in line or "Γö" in line)
            and bool(re.search(r"\d+/\d+", line))
        )

    progress_line = next(
        (line for line in reversed(lines) if is_progress_line(line)),
        "",
    )
    pairs = re.findall(r"(\d+)/(\d+)", progress_line)
    epoch, epochs = (map(int, pairs[0]) if pairs else (0, 0))
    batch, batches = (map(int, pairs[-1]) if len(pairs) > 1 else (0, 0))
    readable_lines = [line for line in lines if not is_noisy_progress_line(line)]
    if epoch and epochs:
        progress_pct = round(batch * 100 / batches, 1) if batches else 0
        readable_lines.append(
            f"Progress: Epoch {epoch}/{epochs} | "
            f"Batch {batch}/{batches} | {progress_pct}% epoch"
        )
    return {
        "log": readable_lines[-35:],
        "epoch": epoch,
        "epochs": epochs,
        "batch": batch,
        "batches": batches,
    }


@app.get("/research/train/status")
def research_training_status():
    state = _read_research_train_state()
    details = _research_log_details()
    task = state.get("task")
    if state.get("status") == "running" and not _pid_is_running(state.get("pid")):
        state["status"] = "failed"
        state["message"] = "Training process is no longer running"
    batch_pct = (
        round(details["batch"] * 100 / details["batches"], 2)
        if details["batches"] else 0
    )
    return {
        **state,
        **details,
        "progress_pct": batch_pct,
        "task_order": RESEARCH_TASKS.get(task, {}).get("order", 0),
        "task_count": len(RESEARCH_TASKS),
        "models": research_status()["models"],
    }


@app.post("/research/train/start")
def start_research_training(
    task: str = "all",
    device: str = "0",
    epochs: int = 0,
    imgsz: int = 0,
    batch: int = 0,
    force: bool = False,
):
    global _research_train_proc
    if task != "all" and task not in RESEARCH_TASKS:
        raise HTTPException(status_code=400, detail="Invalid research training task")
    if device not in {"0", "cpu"}:
        raise HTTPException(status_code=400, detail="Device must be 0 or cpu")
    if epochs < 0 or epochs > 1000:
        raise HTTPException(status_code=400, detail="Epochs must be between 1 and 1000, or 0 for preset")
    if imgsz < 0 or (imgsz and (imgsz < 64 or imgsz > 2048 or imgsz % 32)):
        raise HTTPException(status_code=400, detail="Image size must be a multiple of 32 between 64 and 2048, or 0 for preset")
    if batch < 0 or batch > 512:
        raise HTTPException(status_code=400, detail="Batch must be between 1 and 512, or 0 for preset")
    if _train_state.get("status") in {"starting", "running"}:
        raise HTTPException(status_code=409, detail="Stop legacy training before starting the research pipeline")
    current = _read_research_train_state()
    if current.get("status") == "running" and _pid_is_running(current.get("pid")):
        raise HTTPException(status_code=409, detail="Research training is already running")

    warnings = []
    if device == "0" and torch.cuda.is_available():
        memory_gb = torch.cuda.get_device_properties(0).total_memory / (1024 ** 3)
        if memory_gb < 3 and imgsz > 320:
            warnings.append(
                f"GPU chỉ có {memory_gb:.1f} GB VRAM; image size {imgsz} có nguy cơ hết bộ nhớ."
            )
        if memory_gb < 3 and batch > 1 and task in {"all", "fracture-detector"}:
            warnings.append(
                f"Detector trên GPU {memory_gb:.1f} GB nên dùng batch 1; batch {batch} có thể gây CUDA OOM."
            )
    if warnings and not force:
        raise HTTPException(
            status_code=409,
            detail={
                "requires_confirmation": True,
                "message": "\n\n".join(warnings) + "\n\nBạn có muốn tiếp tục không?",
                "warnings": warnings,
            },
        )

    script = Path(__file__).parent / "run_research_training.ps1"
    creation_flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
    _research_train_proc = subprocess.Popen(
        [
            "powershell.exe",
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            str(script),
            "-Device",
            device,
            "-Task",
            task,
            "-Epochs",
            str(epochs),
            "-ImageSize",
            str(imgsz),
            "-BatchSize",
            str(batch),
        ],
        cwd=Path(__file__).parent,
        creationflags=creation_flags,
    )
    time.sleep(0.25)
    return {
        "success": True,
        "pid": _research_train_proc.pid,
        "task": task,
        "overrides": {"epochs": epochs, "imgsz": imgsz, "batch": batch},
    }


@app.post("/research/train/stop")
def stop_research_training():
    state = _read_research_train_state()
    pid = state.get("pid")
    if state.get("status") != "running" or not _pid_is_running(pid):
        return {"success": False, "message": "No research training is running"}
    subprocess.run(
        ["taskkill", "/PID", str(int(pid)), "/T", "/F"],
        capture_output=True,
        text=True,
        check=False,
    )
    state.update(
        status="stopped",
        message="Research training stopped by user",
        updated_at=datetime.now().isoformat(timespec="seconds"),
    )
    _research_state_path().write_text(
        json.dumps(state, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    return {"success": True}


@app.post("/research/detect")
async def research_detect(file: UploadFile = File(...)):
    research_train_state = _read_research_train_state()
    research_is_running = (
        research_train_state.get("status") == "running"
        and _pid_is_running(research_train_state.get("pid"))
    )
    if _train_state.get("status") in {"starting", "running"} or research_is_running:
        raise HTTPException(
            status_code=409,
            detail="Training is running. Finish or stop training before inference.",
        )
    try:
        image = Image.open(io.BytesIO(await file.read())).convert("RGB")
    except Exception:
        raise HTTPException(status_code=400, detail="Invalid image file")

    fracture_classifier = _get_research_model("fracture_classifier")
    fracture_detector = _get_research_model("fracture_detector")
    abnormality_classifier = _get_research_model("abnormality_classifier")
    if not any((fracture_classifier, fracture_detector, abnormality_classifier)):
        raise HTTPException(
            status_code=503,
            detail=(
                "Research models have not been trained. Run prepare_research_data.py "
                "and train_research.py first."
            ),
        )

    fracture_scores = (
        _classification_scores(fracture_classifier, image)
        if fracture_classifier else {}
    )
    abnormality_scores = (
        _classification_scores(abnormality_classifier, image)
        if abnormality_classifier else {}
    )
    detections: list[dict] = []
    annotated = image.copy()
    if fracture_detector:
        detection_result = fracture_detector.predict(
            image,
            imgsz=320,
            conf=0.25,
            device=RESEARCH_INFERENCE_DEVICE,
            verbose=False,
        )[0]
        import cv2

        canvas = detection_result.orig_img.copy()
        for box in detection_result.boxes:
            confidence = float(box.conf[0])
            bbox = [round(float(value)) for value in box.xyxy[0].tolist()]
            detections.append(
                {"label": "fracture", "confidence": round(confidence, 4), "bbox": bbox}
            )
            x1, y1, x2, y2 = bbox
            cv2.rectangle(canvas, (x1, y1), (x2, y2), (0, 90, 255), 2)
            cv2.putText(
                canvas,
                f"fracture {confidence:.2f}",
                (x1, max(18, y1 - 6)),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.55,
                (0, 90, 255),
                2,
            )
        annotated = Image.fromarray(canvas[..., ::-1])

    fracture_probability = float(fracture_scores.get("fracture", 0.0))
    abnormal_probability = float(abnormality_scores.get("abnormal", 0.0))
    if fracture_probability >= 0.5 or detections:
        preliminary = "Nghi ngờ gãy xương"
    elif abnormal_probability >= 0.5:
        preliminary = "Không thấy gãy rõ; có dấu hiệu bất thường khác"
    else:
        preliminary = "Chưa phát hiện gãy hoặc bất thường rõ"

    output = io.BytesIO()
    annotated.save(output, format="JPEG", quality=90)
    return {
        "research_only": True,
        "warning": "Kết quả nghiên cứu, không thay thế kết luận của bác sĩ.",
        "preliminary_conclusion": preliminary,
        "fracture": {
            "probability": round(fracture_probability, 4),
            "scores": fracture_scores,
            "localizations": detections,
        },
        "other_abnormality": {
            "probability": round(abnormal_probability, 4),
            "scores": abnormality_scores,
            "note": "MURA chỉ cung cấp nhãn bất thường/không bất thường, không định danh bệnh.",
        },
        "models_available": {
            "fracture_classifier": fracture_classifier is not None,
            "fracture_detector": fracture_detector is not None,
            "abnormality_classifier": abnormality_classifier is not None,
        },
        "inference_device": RESEARCH_INFERENCE_DEVICE,
        "annotated_image_base64": base64.b64encode(output.getvalue()).decode(),
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
    registry_models = _research_registry().get("models", {})
    active_research = {
        key: Path(entry.get("path", "")).name
        for key, entry in registry_models.items()
        if entry.get("path")
    }
    research_models = []
    for f in sorted(
        RESEARCH_MODELS_DIR.glob("*.pt"),
        key=lambda p: p.stat().st_mtime,
        reverse=True,
    ):
        research_key = next(
            (key for key in RESEARCH_TASKS_BY_KEY if f.name.startswith(f"{key}_")),
            None,
        )
        # Also support the original non-versioned filenames.
        if research_key is None and f.stem in RESEARCH_TASKS_BY_KEY:
            research_key = f.stem
        if research_key is None:
            continue
        research_models.append({
            "name": f.name,
            "size_mb": round(f.stat().st_size / 1024 / 1024, 1),
            "active": active_research.get(research_key) == f.name,
            "is_origin": False,
            "kind": "research",
            "research_key": research_key,
            "modified": datetime.fromtimestamp(f.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
        })
    models = trained + ([origin] if origin else [])
    return {"models": research_models + models}


RESEARCH_TASKS_BY_KEY = {
    "fracture_classifier": "fracture-classifier",
    "fracture_detector": "fracture-detector",
    "abnormality_classifier": "abnormality-classifier",
}


@app.post("/research/models/activate/{research_key}/{filename}")
def activate_research_model(research_key: str, filename: str):
    global _research_models
    if research_key not in RESEARCH_TASKS_BY_KEY:
        raise HTTPException(status_code=404, detail="Unknown research model role")
    if Path(filename).name != filename:
        raise HTTPException(status_code=400, detail="Invalid filename")
    target = RESEARCH_MODELS_DIR / filename
    if not target.is_file() or target.suffix.lower() != ".pt":
        raise HTTPException(status_code=404, detail=f"{filename} not found")
    if not (
        filename == f"{research_key}.pt"
        or filename.startswith(f"{research_key}_")
    ):
        raise HTTPException(
            status_code=400,
            detail=f"{filename} is not a {research_key} model",
        )

    registry = _research_registry()
    models = registry.setdefault("models", {})
    previous = models.get(research_key, {})
    models[research_key] = {
        **previous,
        "task_name": RESEARCH_TASKS_BY_KEY[research_key],
        "path": str(target.relative_to(Path(__file__).parent)).replace("\\", "/"),
        "activated_at": datetime.now().isoformat(timespec="seconds"),
    }
    RESEARCH_MODELS_DIR.mkdir(parents=True, exist_ok=True)
    RESEARCH_REGISTRY_FILE.write_text(
        json.dumps(registry, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )
    _research_models.pop(research_key, None)
    return {
        "success": True,
        "research_key": research_key,
        "active": filename,
    }


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

IMAGE_SUFFIXES = {".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tif", ".tiff"}


def _dataset_image_count(data: str) -> int:
    """Read a YOLO data YAML and count training images before training starts."""
    yaml_path = Path(data)
    if not yaml_path.is_absolute():
        yaml_path = Path(__file__).parent / yaml_path
    if not yaml_path.exists():
        return 0

    config = yaml.safe_load(yaml_path.read_text(encoding="utf-8")) or {}
    train_source = config.get("train")
    if not isinstance(train_source, str):
        return 0

    train_path = Path(train_source)
    if not train_path.is_absolute():
        train_path = yaml_path.parent / train_path
    if train_path.is_dir():
        return sum(1 for p in train_path.rglob("*") if p.is_file() and p.suffix.lower() in IMAGE_SUFFIXES)
    if train_path.is_file() and train_path.suffix.lower() == ".txt":
        return sum(1 for line in train_path.read_text(encoding="utf-8").splitlines() if line.strip())
    return 0

def _run_training(data: str, epochs: int, base_model: str, imgsz: int, batch: int, device: str):
    global _train_state
    _train_state["log"] = []
    _train_state["status"] = "running"
    _train_state["new_model"] = None
    _train_state["device"] = device
    _train_state["current_epoch"] = 0
    _train_state["total_epochs"] = epochs
    _train_state["progress_pct"] = 0
    _train_state["images_done"] = 0
    dataset_size = _dataset_image_count(data)
    _train_state["images_total"] = dataset_size * epochs
    _train_state["dataset_size"] = dataset_size
    _train_state["current_batch"] = 0
    _train_state["batches_per_epoch"] = math.ceil(dataset_size / batch) if dataset_size and batch > 0 else 0
    _train_state["elapsed_seconds"] = 0
    _train_state["eta_seconds"] = None
    _train_state["best_pt"] = None
    _train_state["run_dir"] = None

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

    child_env = os.environ.copy()
    child_env["PYTHONIOENCODING"] = "utf-8"
    child_env["PYTHONUTF8"] = "1"
    proc = subprocess.Popen(
        cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding="utf-8",
        errors="replace",
        bufsize=1,
        env=child_env,
        cwd=Path(__file__).parent,
    )
    _train_state["proc"] = proc

    for line in proc.stdout:
        line = line.rstrip()
        result_marker = "XRAY_TRAINING_RESULT "
        result_index = line.find(result_marker)
        if result_index >= 0:
            try:
                result_text = line[result_index + len(result_marker):].lstrip()
                training_result, _ = json.JSONDecoder().raw_decode(result_text)
                _train_state["best_pt"] = training_result["best_pt"]
                _train_state["run_dir"] = training_result["run_dir"]
            except (KeyError, TypeError, ValueError, json.JSONDecodeError):
                _train_state["log"].append(f"Invalid training result message: {line}")
            continue
        marker = "XRAY_PROGRESS "
        marker_index = line.find(marker)
        if marker_index >= 0:
            try:
                progress_text = line[marker_index + len(marker):].lstrip()
                # raw_decode tolerates terminal/progress-bar text after the JSON object.
                progress, _ = json.JSONDecoder().raw_decode(progress_text)
                _train_state["current_epoch"] = progress["epoch"]
                _train_state["total_epochs"] = progress["epochs"]
                _train_state["current_batch"] = progress["batch"]
                _train_state["batches_per_epoch"] = progress["batches_per_epoch"]
                _train_state["images_done"] = progress["images_done"]
                _train_state["images_total"] = progress["images_total"]
                _train_state["dataset_size"] = progress["dataset_size"]
                _train_state["elapsed_seconds"] = progress["elapsed_seconds"]
                _train_state["eta_seconds"] = progress["eta_seconds"]
                _train_state["progress_pct"] = progress["progress_pct"]
            except (KeyError, TypeError, ValueError, json.JSONDecodeError):
                _train_state["log"].append(f"Invalid progress message: {line}")
            continue
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
        best_pt = Path(_train_state["best_pt"]) if _train_state.get("best_pt") else None
        run_dir = Path(_train_state["run_dir"]) if _train_state.get("run_dir") else None
        if best_pt and run_dir and best_pt.is_file() and run_dir.is_dir():
            ts = datetime.now().strftime("%Y%m%d_%H%M%S")
            dest_name = f"trained_{ts}.pt"
            shutil.copy2(best_pt, MODELS_DIR / dest_name)
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
            _train_state["log"].append(
                "❌ Training process finished but did not return a valid best.pt; no model was copied."
            )
    else:
        _train_state["status"] = "error"
        _train_state["log"].append(f"❌ Training failed (exit {proc.returncode})")


@app.post("/train/start")
def start_training(
    background_tasks: BackgroundTasks,
    data: str = "dataset/data.yaml",
    epochs: int = 100,
    base_model: str = "yolov8n.pt",
    imgsz: int = 320,
    batch: int = 1,
    device: str = "0",
    force: bool = False,
):
    if _train_state["status"] in {"starting", "running"}:
        raise HTTPException(status_code=409, detail="Training already in progress")
    research_state = _read_research_train_state()
    if research_state.get("status") == "running" and _pid_is_running(research_state.get("pid")):
        raise HTTPException(
            status_code=409,
            detail="Research training is running. Stop it before starting legacy training.",
        )

    uses_cuda = device == "auto" or device.isdigit() or device.startswith("cuda")
    if uses_cuda and torch.cuda.is_available():
        gpu_memory_gb = torch.cuda.get_device_properties(0).total_memory / (1024 ** 3)
        warnings = []
        if gpu_memory_gb < 3:
            model_stem = Path(base_model).stem.lower()
            if model_stem.startswith(("yolov8s", "yolov8m", "yolov8l", "yolov8x")):
                warnings.append(
                    f"GPU chỉ có {gpu_memory_gb:.1f} GB VRAM; {base_model} có thể quá lớn. "
                    "Khuyến nghị dùng yolov8n.pt."
                )
            if batch > 1 or imgsz > 320:
                warnings.append(
                    f"Cấu hình batch={batch}, image size={imgsz} có nguy cơ hết VRAM. "
                    "Khuyến nghị Batch size = 1 và Image size = 320."
                )
        if warnings and not force:
            raise HTTPException(
                status_code=409,
                detail={
                    "requires_confirmation": True,
                    "message": "\n\n".join(warnings) + "\n\nBạn có muốn tiếp tục training không?",
                    "warnings": warnings,
                },
            )

    _train_state["status"] = "starting"
    _release_inference_model()
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
        "images_done": _train_state.get("images_done", 0),
        "images_total": _train_state.get("images_total", 0),
        "dataset_size": _train_state.get("dataset_size", 0),
        "current_batch": _train_state.get("current_batch", 0),
        "batches_per_epoch": _train_state.get("batches_per_epoch", 0),
        "elapsed_seconds": _train_state.get("elapsed_seconds", 0),
        "eta_seconds": _train_state.get("eta_seconds"),
    }


# ─── Admin UI ────────────────────────────────────────────────────────────────

@app.get("/admin", response_class=HTMLResponse)
def admin_ui():
    return Path(__file__).parent.joinpath("static", "admin.html").read_text(encoding="utf-8")
