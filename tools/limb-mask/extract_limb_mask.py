#!/usr/bin/env python3
from __future__ import annotations

import argparse
from pathlib import Path

try:
    from PIL import Image
except ImportError as exc:
    raise SystemExit("Pillow is required: python -m pip install Pillow") from exc


EXPECTED_SIZE = (463, 400)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Derive the shared standard Don limb mask for the WebUI preview."
    )
    parser.add_argument(
        "--standard-art",
        default="TaikoWebUI/wwwroot/images/Costumes/body/body-0000.webp",
        help="Standard Don body preview WebP.",
    )
    parser.add_argument(
        "--body-mask",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/body-bodymask-0000.webp",
        help="Existing standard Don body color mask WebP.",
    )
    parser.add_argument(
        "--face-mask",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/body-facemask-0000.webp",
        help="Existing standard Don face color mask WebP.",
    )
    parser.add_argument(
        "--out",
        default="TaikoWebUI/wwwroot/images/Costumes/masks/standard-limbmask-0000.webp",
        help="Output WebP mask path.",
    )
    return parser.parse_args()


def load_rgba(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    if image.size != EXPECTED_SIZE:
        raise SystemExit(f"{path} has size {image.size}; expected {EXPECTED_SIZE}.")
    return image


def is_limb_surface(r: int, g: int, b: int, a: int) -> bool:
    if a < 80:
        return False

    # Standard art limb surfaces are the light cream regions left after the
    # existing body and face masks are excluded. This avoids recoloring outlines.
    return r >= 180 and g >= 160 and b >= 130 and abs(r - g) <= 45 and abs(g - b) <= 70


def derive_mask(standard_art: Image.Image, body_mask: Image.Image, face_mask: Image.Image) -> Image.Image:
    output = Image.new("RGBA", EXPECTED_SIZE, (0, 0, 0, 0))
    out_pixels = output.load()
    standard_pixels = standard_art.load()
    body_alpha = body_mask.getchannel("A").load()
    face_alpha = face_mask.getchannel("A").load()

    for y in range(EXPECTED_SIZE[1]):
        for x in range(EXPECTED_SIZE[0]):
            if body_alpha[x, y] > 0 or face_alpha[x, y] > 0:
                continue

            r, g, b, a = standard_pixels[x, y]
            if is_limb_surface(r, g, b, a):
                out_pixels[x, y] = (0, 0, 0, 255)

    return output


def main() -> None:
    args = parse_args()
    standard_art = load_rgba(Path(args.standard_art))
    body_mask = load_rgba(Path(args.body_mask))
    face_mask = load_rgba(Path(args.face_mask))

    mask = derive_mask(standard_art, body_mask, face_mask)
    alpha_bbox = mask.getchannel("A").getbbox()
    if alpha_bbox is None:
        raise SystemExit("Derived limb mask is empty.")

    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    mask.save(out, format="WEBP", lossless=True, exact=True)
    print(f"wrote {out} size={mask.size} alpha_bbox={alpha_bbox}")


if __name__ == "__main__":
    main()
