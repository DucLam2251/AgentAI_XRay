"""Train and register the FracAtlas/MURA research models."""

from __future__ import annotations

import argparse
import json
import shutil
from datetime import datetime
from pathlib import Path

from ultralytics import YOLO

from train import resolve_device


ROOT = Path(__file__).resolve().parent
PREPARED = ROOT / "datasets" / "prepared"
MODEL_DIR = ROOT / "models" / "research"
REGISTRY = MODEL_DIR / "registry.json"
PROJECT = ROOT / "runs" / "research"

TASKS = {
    "fracture-classifier": {
        "task": "classify",
        "data": PREPARED / "fracatlas_classification",
        "base": "yolov8n-cls.pt",
        "registry_key": "fracture_classifier",
        "epochs": 30,
        "imgsz": 224,
        "batch": 8,
    },
    "fracture-detector": {
        "task": "detect",
        "data": PREPARED / "fracatlas_detection" / "data.yaml",
        "base": "yolov8n.pt",
        "registry_key": "fracture_detector",
        "epochs": 50,
        "imgsz": 320,
        "batch": 1,
    },
    "abnormality-classifier": {
        "task": "classify",
        "data": PREPARED / "mura_abnormality",
        "base": "yolov8n-cls.pt",
        "registry_key": "abnormality_classifier",
        "epochs": 15,
        "imgsz": 224,
        "batch": 8,
    },
}


def load_registry() -> dict:
    if REGISTRY.exists():
        return json.loads(REGISTRY.read_text(encoding="utf-8"))
    return {"research_only": True, "models": {}}


def train_task(
    name: str,
    epochs: int | None,
    imgsz: int | None,
    batch: int | None,
    device: str,
) -> dict:
    config = TASKS[name]
    epochs = epochs or config["epochs"]
    imgsz = imgsz or config["imgsz"]
    batch = batch or config["batch"]
    if not config["data"].exists():
        raise FileNotFoundError(f"Prepared dataset not found: {config['data']}. Run prepare_research_data.py first.")
    actual_device, device_desc = resolve_device(device)
    print(f"[{name}] device={device_desc}, data={config['data']}")
    model = YOLO(config["base"])
    run_name = f"{name}-{datetime.now():%Y%m%d-%H%M%S}"
    results = model.train(
        data=str(config["data"]),
        epochs=epochs,
        imgsz=imgsz,
        batch=batch,
        device=actual_device,
        project=str(PROJECT),
        name=run_name,
        patience=max(5, min(20, epochs // 4)),
        save=True,
        plots=True,
        workers=2,
        amp=True,
        seed=42,
        deterministic=True,
    )
    best = Path(results.save_dir) / "weights" / "best.pt"
    if not best.exists():
        raise RuntimeError(f"Training completed without best.pt: {results.save_dir}")

    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    # Keep every successful training result so the admin UI can list and
    # reactivate older research models instead of overwriting the last one.
    destination = MODEL_DIR / f"{config['registry_key']}_{datetime.now():%Y%m%d_%H%M%S}.pt"
    shutil.copy2(best, destination)
    return {
        "task_name": name,
        "task": config["task"],
        "path": str(destination.relative_to(ROOT)).replace("\\", "/"),
        "source_weights": str(best),
        "trained_at": datetime.now().isoformat(timespec="seconds"),
        "epochs_requested": epochs,
        "imgsz": imgsz,
        "batch": batch,
        "device": device,
        "classes": model.names,
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("task", choices=(*TASKS.keys(), "all"))
    parser.add_argument("--epochs", type=int, help="Override the per-task preset")
    parser.add_argument("--imgsz", type=int, help="Override the per-task preset")
    parser.add_argument("--batch", type=int, help="Override the per-task preset")
    parser.add_argument("--device", default="0")
    args = parser.parse_args()

    names = list(TASKS) if args.task == "all" else [args.task]
    registry = load_registry()
    for name in names:
        trained = train_task(name, args.epochs, args.imgsz, args.batch, args.device)
        registry["models"][TASKS[name]["registry_key"]] = trained
        MODEL_DIR.mkdir(parents=True, exist_ok=True)
        REGISTRY.write_text(json.dumps(registry, indent=2, ensure_ascii=False), encoding="utf-8")
        print(f"Registered {name}: {trained['path']}")


if __name__ == "__main__":
    main()
