"""Prepare FracAtlas and MURA for independent research training tasks.

Outputs use hard links when possible, so preparing 40k MURA images does not
duplicate the image bytes on disk.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import os
import shutil
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parent
DATASETS = ROOT / "datasets"
FRAC_ROOT = DATASETS / "fracatlas" / "FracAtlas"
MURA_ROOT = DATASETS / "mura" / "MURA-v1.1"
OUTPUT_ROOT = DATASETS / "prepared"


def stable_bucket(value: str, modulo: int = 100) -> int:
    return int(hashlib.sha256(value.encode("utf-8")).hexdigest()[:8], 16) % modulo


def link_file(source: Path, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists():
        return
    try:
        os.link(source, destination)
    except OSError:
        shutil.copy2(source, destination)


def read_single_column_csv(path: Path) -> set[str]:
    with path.open(newline="", encoding="utf-8-sig") as file:
        reader = csv.DictReader(file)
        return {row["image_id"].strip() for row in reader}


def prepare_fracatlas() -> dict:
    dataset_csv = FRAC_ROOT / "dataset.csv"
    images_root = FRAC_ROOT / "images"
    labels_root = FRAC_ROOT / "Annotations" / "YOLO"
    split_root = FRAC_ROOT / "Utilities" / "Fracture Split"
    positive_splits = {
        "train": read_single_column_csv(split_root / "train.csv"),
        "val": read_single_column_csv(split_root / "valid.csv"),
        "test": read_single_column_csv(split_root / "test.csv"),
    }
    positive_lookup = {
        image_id: split for split, image_ids in positive_splits.items() for image_id in image_ids
    }

    classification_root = OUTPUT_ROOT / "fracatlas_classification"
    detection_root = OUTPUT_ROOT / "fracatlas_detection"
    manifest_path = OUTPUT_ROOT / "manifests" / "fracatlas.csv"
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    counts: Counter[str] = Counter()

    with dataset_csv.open(newline="", encoding="utf-8-sig") as source_file, manifest_path.open(
        "w", newline="", encoding="utf-8"
    ) as manifest_file:
        reader = csv.DictReader(source_file)
        fieldnames = [
            "image_id",
            "source",
            "split",
            "fracture_label",
            "anatomy",
            "view",
            "hardware",
            "adult_status",
            "bbox_available",
        ]
        writer = csv.DictWriter(manifest_file, fieldnames=fieldnames)
        writer.writeheader()

        for row in reader:
            image_id = row["image_id"].strip()
            fractured = row["fractured"] == "1"
            if fractured:
                split = positive_lookup.get(image_id)
                if split is None:
                    raise ValueError(f"Positive image missing official split: {image_id}")
            else:
                bucket = stable_bucket(image_id)
                split = "train" if bucket < 80 else ("val" if bucket < 90 else "test")

            class_name = "fracture" if fractured else "no_fracture"
            source_image = images_root / ("Fractured" if fractured else "Non_fractured") / image_id
            if not source_image.exists():
                raise FileNotFoundError(source_image)

            link_file(source_image, classification_root / split / class_name / image_id)
            link_file(source_image, detection_root / "images" / split / image_id)

            source_label = labels_root / f"{Path(image_id).stem}.txt"
            destination_label = detection_root / "labels" / split / f"{Path(image_id).stem}.txt"
            if source_label.exists():
                link_file(source_label, destination_label)
            else:
                destination_label.parent.mkdir(parents=True, exist_ok=True)
                destination_label.touch(exist_ok=True)

            anatomy = next(
                (name for name in ("hand", "leg", "hip", "shoulder", "mixed") if row[name] == "1"),
                "unknown",
            )
            views = [name for name in ("frontal", "lateral", "oblique") if row[name] == "1"]
            writer.writerow(
                {
                    "image_id": image_id,
                    "source": "fracatlas",
                    "split": split,
                    "fracture_label": int(fractured),
                    "anatomy": anatomy,
                    "view": "+".join(views) or "unknown",
                    "hardware": row["hardware"],
                    # FracAtlas does not expose per-image age in dataset.csv.
                    "adult_status": "unknown",
                    "bbox_available": int(fractured and source_label.exists() and source_label.stat().st_size > 0),
                }
            )
            counts[f"{split}/{class_name}"] += 1

    data_yaml = detection_root / "data.yaml"
    data_yaml.write_text(
        "\n".join(
            [
                f"path: {detection_root.resolve().as_posix()}",
                "train: images/train",
                "val: images/val",
                "test: images/test",
                "names:",
                "  0: fracture",
                "",
            ]
        ),
        encoding="utf-8",
    )
    return {"counts": dict(sorted(counts.items())), "manifest": str(manifest_path), "data_yaml": str(data_yaml)}


def mura_study_labels(path: Path) -> dict[str, int]:
    labels: dict[str, int] = {}
    with path.open(newline="", encoding="utf-8-sig") as file:
        for study_path, label in csv.reader(file):
            labels[study_path.rstrip("/")] = int(label)
    return labels


def mura_image_paths(path: Path) -> list[str]:
    with path.open(newline="", encoding="utf-8-sig") as file:
        return [row[0].strip() for row in csv.reader(file) if row]


def mura_patient_id(relative_path: str) -> str:
    return next((part for part in Path(relative_path).parts if part.startswith("patient")), "unknown")


def mura_anatomy(relative_path: str) -> str:
    return next((part.removeprefix("XR_").lower() for part in Path(relative_path).parts if part.startswith("XR_")), "unknown")


def prepare_mura() -> dict:
    train_labels = mura_study_labels(MURA_ROOT / "train_labeled_studies.csv")
    valid_labels = mura_study_labels(MURA_ROOT / "valid_labeled_studies.csv")
    records = [
        ("source_train", path, train_labels) for path in mura_image_paths(MURA_ROOT / "train_image_paths.csv")
    ] + [("source_valid", path, valid_labels) for path in mura_image_paths(MURA_ROOT / "valid_image_paths.csv")]

    output_root = OUTPUT_ROOT / "mura_abnormality"
    manifest_path = OUTPUT_ROOT / "manifests" / "mura.csv"
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    counts: Counter[str] = Counter()
    missing: list[str] = []

    with manifest_path.open("w", newline="", encoding="utf-8") as manifest_file:
        fieldnames = [
            "image_path",
            "source",
            "patient_id",
            "study_id",
            "split",
            "abnormality_label",
            "anatomy",
        ]
        writer = csv.DictWriter(manifest_file, fieldnames=fieldnames)
        writer.writeheader()

        for source_split, relative_path, labels in records:
            normalized = relative_path.replace("\\", "/")
            study_path = normalized.rsplit("/", 1)[0]
            label = labels.get(study_path)
            if label is None:
                raise ValueError(f"No study label for {relative_path}")
            patient_id = mura_patient_id(normalized)
            if source_split == "source_valid":
                split = "test"
            else:
                split = "val" if stable_bucket(patient_id) < 10 else "train"

            source_image = DATASETS / "mura" / Path(normalized)
            if not source_image.exists():
                missing.append(normalized)
                continue
            class_name = "abnormal" if label else "normal"
            destination_name = (
                f"{mura_anatomy(normalized)}_{patient_id}_"
                f"{Path(study_path).name}_{Path(normalized).name}"
            )
            link_file(source_image, output_root / split / class_name / destination_name)
            writer.writerow(
                {
                    "image_path": normalized,
                    "source": "mura",
                    "patient_id": patient_id,
                    "study_id": study_path,
                    "split": split,
                    "abnormality_label": label,
                    "anatomy": mura_anatomy(normalized),
                }
            )
            counts[f"{split}/{class_name}"] += 1

    if missing:
        raise FileNotFoundError(f"MURA extraction is incomplete; {len(missing)} paths are missing. First: {missing[0]}")
    return {"counts": dict(sorted(counts.items())), "manifest": str(manifest_path)}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", choices=("all", "fracatlas", "mura"), default="all")
    args = parser.parse_args()
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)
    summary: dict[str, dict] = {}
    if args.dataset in {"all", "fracatlas"}:
        summary["fracatlas"] = prepare_fracatlas()
    if args.dataset in {"all", "mura"}:
        summary["mura"] = prepare_mura()
    summary_path = OUTPUT_ROOT / "summary.json"
    summary_path.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps(summary, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
