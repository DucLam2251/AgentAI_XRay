"""
Training pipeline for bone fracture detection.

Usage:
    python train.py --data path/to/data.yaml [--epochs 100] [--model yolov8s.pt] [--imgsz 640]

After training, run deploy.py to promote best.pt to production.
"""

import argparse
import json
import shutil
import time
from pathlib import Path

import torch
from ultralytics import YOLO

def resolve_device(requested: str) -> tuple[str, str]:
    """Resolve a UI/CLI device value to a device accepted by Ultralytics."""
    req = (requested or "auto").strip().lower()

    if req == "cpu":
        return "cpu", "CPU"

    if req == "auto":
        # Prefer CUDA because it is the best-supported GPU backend in Ultralytics.
        if torch.cuda.is_available():
            return "0", f"NVIDIA GPU 0 ({torch.cuda.get_device_name(0)})"
        if hasattr(torch, "xpu") and torch.xpu.is_available():
            return "xpu:0", f"Intel GPU 0 ({torch.xpu.get_device_name(0)})"
        return "cpu", "CPU (no supported GPU detected)"

    # Ultralytics commonly uses "0", "1", ... for CUDA device indices.
    if req.isdigit() or req == "cuda" or req.startswith("cuda:"):
        index_text = req if req.isdigit() else (req.split(":", 1)[1] if ":" in req else "0")
        if not index_text.isdigit():
            raise ValueError(f"Invalid CUDA device: {requested}")
        index = int(index_text)
        if not torch.cuda.is_available():
            raise RuntimeError(
                "CUDA GPU was requested, but PyTorch cannot detect CUDA. "
                "Install a CUDA-enabled PyTorch build and verify the NVIDIA driver."
            )
        if index >= torch.cuda.device_count():
            raise RuntimeError(
                f"CUDA device {index} was requested, but only "
                f"{torch.cuda.device_count()} CUDA device(s) are available."
            )
        return str(index), f"NVIDIA GPU {index} ({torch.cuda.get_device_name(index)})"

    if req == "xpu" or req.startswith("xpu:"):
        index_text = req.split(":", 1)[1] if ":" in req else "0"
        if not index_text.isdigit():
            raise ValueError(f"Invalid XPU device: {requested}")
        index = int(index_text)
        if not hasattr(torch, "xpu") or not torch.xpu.is_available():
            raise RuntimeError(
                "Intel XPU was requested, but PyTorch cannot detect XPU. "
                "Install an XPU-enabled PyTorch build and the required Intel drivers."
            )
        if index >= torch.xpu.device_count():
            raise RuntimeError(
                f"XPU device {index} was requested, but only "
                f"{torch.xpu.device_count()} XPU device(s) are available."
            )
        return f"xpu:{index}", f"Intel GPU {index} ({torch.xpu.get_device_name(index)})"

    raise ValueError(
        f"Unsupported device '{requested}'. Use auto, cpu, 0, cuda:0, xpu, or xpu:0."
    )


def train(data: str, epochs: int, base_model: str, imgsz: int, batch: int, name: str, device: str):
    actual_device, device_desc = resolve_device(device)
    print(f"Requested device: {device}")
    print(f"Using device: {device_desc}")

    model = YOLO(base_model)

    progress = {
        "started_at": 0.0,
        "batches_done_in_epoch": 0,
    }

    def emit_progress(trainer):
        """Emit a machine-readable progress line consumed by the FastAPI process."""
        dataset_size = len(trainer.train_loader.dataset)
        batches_per_epoch = len(trainer.train_loader)
        progress["batches_done_in_epoch"] += 1

        epoch = trainer.epoch + 1
        images_in_epoch = min(
            progress["batches_done_in_epoch"] * trainer.batch_size,
            dataset_size,
        )
        images_done = min((epoch - 1) * dataset_size + images_in_epoch, dataset_size * trainer.epochs)
        images_total = dataset_size * trainer.epochs
        elapsed = max(0.0, time.monotonic() - progress["started_at"])
        eta = (elapsed / images_done * (images_total - images_done)) if images_done else None

        payload = {
            "epoch": epoch,
            "epochs": trainer.epochs,
            "batch": progress["batches_done_in_epoch"],
            "batches_per_epoch": batches_per_epoch,
            "images_done": images_done,
            "images_total": images_total,
            "dataset_size": dataset_size,
            "elapsed_seconds": round(elapsed),
            "eta_seconds": round(eta) if eta is not None else None,
            "progress_pct": round(images_done * 100 / images_total, 4) if images_total else 0,
        }
        print("XRAY_PROGRESS " + json.dumps(payload), flush=True)

    def progress_train_start(trainer):
        progress["started_at"] = time.monotonic()

    def progress_epoch_start(trainer):
        progress["batches_done_in_epoch"] = 0

    model.add_callback("on_train_start", progress_train_start)
    model.add_callback("on_train_epoch_start", progress_epoch_start)
    model.add_callback("on_train_batch_end", emit_progress)

    results = model.train(
        data=data,
        epochs=epochs,
        imgsz=imgsz,
        batch=batch,
        device=actual_device,
        name=name,
        patience=20,        # early stopping
        save=True,
        plots=True,
    )

    # Path to the best weights produced by this run
    best_pt = Path(results.save_dir) / "weights" / "best.pt"
    print(f"\nTraining complete. Best weights: {best_pt}")
    print(f"mAP50: {results.results_dict.get('metrics/mAP50(B)', 'N/A')}")
    return best_pt


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--data", required=True, help="Path to data.yaml")
    parser.add_argument("--epochs", type=int, default=100)
    parser.add_argument("--model", default="yolov8s.pt", help="Base pretrained model")
    parser.add_argument("--imgsz", type=int, default=640)
    parser.add_argument("--batch", type=int, default=16)
    parser.add_argument("--device", default="auto", help="Device: auto, cpu, 0, cuda:0, ...")
    parser.add_argument("--name", default="bone_fracture", help="Run name (saved under runs/detect/)")
    args = parser.parse_args()

    best_pt = train(args.data, args.epochs, args.model, args.imgsz, args.batch, args.name, args.device)
    result_payload = {
        "best_pt": str(best_pt.resolve()),
        "run_dir": str(best_pt.resolve().parent.parent),
    }
    print("XRAY_TRAINING_RESULT " + json.dumps(result_payload), flush=True)
    print(f"\nNext step: python deploy.py --weights {best_pt}")
