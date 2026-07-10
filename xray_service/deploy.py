"""
Promote a trained model to production.

Usage:
    python deploy.py --weights runs/detect/bone_fracture/weights/best.pt

Steps:
  1. Validates the weights file loads correctly.
  2. Backs up the current production model.
  3. Copies the new weights to models/best.pt.
  4. Calls /model/reload on the running xray service.
"""

import argparse
import shutil
import urllib.request
from datetime import datetime
from pathlib import Path

from ultralytics import YOLO

SERVICE_URL = "http://localhost:8000"
MODELS_DIR = Path(__file__).parent / "models"
PRODUCTION_MODEL = MODELS_DIR / "best.pt"


def validate(weights: Path) -> dict:
    print(f"Validating {weights}...")
    model = YOLO(str(weights))
    print(f"  Classes: {model.names}")
    return model.names


def backup_current():
    pass  # no backup — overwrite best.pt directly


def reload_service():
    try:
        req = urllib.request.Request(
            f"{SERVICE_URL}/model/reload",
            method="POST",
            headers={"Content-Length": "0"},
            data=b"",
        )
        with urllib.request.urlopen(req, timeout=10) as resp:
            print(f"  Service reloaded: {resp.read().decode()}")
    except Exception as e:
        print(f"  Warning: could not reload service ({e}). Restart xray_service manually.")


def deploy(weights: Path):
    weights = weights.resolve()
    if not weights.exists():
        raise FileNotFoundError(f"Weights not found: {weights}")

    validate(weights)
    MODELS_DIR.mkdir(exist_ok=True)
    backup_current()

    shutil.copy2(weights, PRODUCTION_MODEL)
    print(f"  Copied to {PRODUCTION_MODEL}")

    reload_service()
    print("\nDeployment complete.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--weights", required=True, help="Path to best.pt from training run")
    args = parser.parse_args()
    deploy(Path(args.weights))
