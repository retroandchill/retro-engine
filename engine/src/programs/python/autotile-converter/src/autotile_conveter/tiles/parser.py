# @file parser.py
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
import os
from pathlib import Path
from typing import Iterable

from PIL import Image
from PIL.ImageFile import ImageFile

from .formats import TileFormat

RMXP_TILE_SIZE = 32


def collect_source_tiles(path: str) -> Iterable[Path]:
    target_dir = Path(path)
    return (p for p in target_dir.iterdir() if p.is_file() and p.suffix == ".png")


def parse_autotiles(input_path: str, output_path: str, tile_format: TileFormat):
    os.makedirs(output_path, exist_ok=True)
    for file_path in collect_source_tiles(input_path):
        try:
            convert_autotile(file_path, output_path, tile_format)
        except Exception as e:
            print(f"Failed to convert {file_path}: {e}")


def convert_autotile(input_path: Path, output_dir: str, tile_format: TileFormat):
    with Image.open(input_path) as source_image:
        match tile_format:
            case 'rmxp':
                convert_rmxp_autotile(input_path, source_image, output_dir)
            case 'rmvx':
                convert_rmvx_autotile(input_path, source_image, output_dir)
            case 'rmmv':
                convert_rmmv_autotile(input_path, source_image, output_dir)


def convert_rmxp_autotile(input_path: Path, source_image: ImageFile, output_dir: str):
    if source_image.height == RMXP_TILE_SIZE:
        convert_single_row_rmxp_autotile(input_path, source_image, output_dir)
    elif source_image.height == RMXP_TILE_SIZE * 4:
        pass
    else:
        raise ValueError(f"Unsupported image height: {source_image.height}")


def convert_single_row_rmxp_autotile(input_path: Path, source_image: ImageFile, output_dir: str):
    if source_image.width % RMXP_TILE_SIZE != 0:
        raise ValueError(f"Unsupported image width: {source_image.width}")

    source_image.save(os.path.join(output_dir, input_path.name))


def convert_standard_rmxp_autotile(input_path: Path, source_image: ImageFile, output_dir: str):
    raise NotImplementedError()


def convert_rmvx_autotile(input_path: Path, source_image: ImageFile, output_dir: str):
    raise NotImplementedError()


def convert_rmmv_autotile(input_path: Path, source_image: ImageFile, output_dir: str):
    raise NotImplementedError()
