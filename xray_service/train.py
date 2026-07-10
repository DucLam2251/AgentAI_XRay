"""
Training pipeline for bone fracture detection.

Usage:
    python train.py --data path/to/data.yaml [--epochs 100] [--model yolov8s.pt] [--imgsz 640]

After training, run deploy.py to promote best.pt to production.
"""

import argparse
import shutil
from pathlib import Path

import torch
from ultralytics import YOLO


def resolve_device(requested: str) -> tuple[str, str]:
    req = (requested or "auto").strip().lower()

    if req == "cpu":
        return "cpu", "CPU"

    if req == "auto":
        if torch.cuda.is_available() and torch.cuda.device_count() > 0:
            gpu_name = torch.cuda.get_device_name(0)
            return "0", f"GPU cuda:0 ({gpu_name})"
        return "cpu", "CPU (CUDA not available)"

    if req.isdigit():
        idx = int(req)
        if torch.cuda.is_available() and torch.cuda.device_count() > idx:
            gpu_name = torch.cuda.get_device_name(idx)
            return str(idx), f"GPU cuda:{idx} ({gpu_name})"
        return "cpu", f"CPU (requested GPU {idx} not available)"

    if req.startswith("cuda:"):
        try:
            idx = int(req.split(":", 1)[1])
        except ValueError:
            return "cpu", f"CPU (invalid device '{requested}')"
        if torch.cuda.is_available() and torch.cuda.device_count() > idx:
            gpu_name = torch.cuda.get_device_name(idx)
            return str(idx), f"GPU cuda:{idx} ({gpu_name})"
        return "cpu", f"CPU (requested {requested} not available)"

    return "cpu", f"CPU (unsupported device '{requested}')"


def train(data: str, epochs: int, base_model: str, imgsz: int, batch: int, name: str, device: str):
    actual_device, device_desc = resolve_device(device)
    print(f"Requested device: {device}")
    print(f"Using device: {device_desc}")

    model = YOLO(base_model)

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
    print(f"\nNext step: python deploy.py --weights {best_pt}")
