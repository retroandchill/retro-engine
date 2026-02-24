# @file main.py
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
from pydantic import BaseModel, Field
from tap import tapify

from tiles.formats import TileFormat
from tiles.parser import parse_autotiles


class ProgramArgs(BaseModel):
    input_dir: str = Field(
        description='The input directory for autotile conversion')
    output_dir: str = Field(
        description='The output directory for autotile conversion')
    tile_format: TileFormat = Field(
        default='rmxp', description='The tile format to use for the output files')


def main(input_dir: str, output_dir: str, tile_format: TileFormat):
    parse_autotiles(input_dir, output_dir, tile_format)


if __name__ == "__main__":
    args = tapify(ProgramArgs, underscores_to_dashes=True)
    main(args.input_dir, args.output_dir, args.tile_format)
