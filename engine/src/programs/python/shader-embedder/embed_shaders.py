from pathlib import Path
from typing import TypedDict, Collection

import numpy
from pybars import Compiler
from pydantic import BaseModel, Field
from tap import tapify


class ProgramArgs(BaseModel):
    input_files: list[str] = Field(description='The input files to embed')
    output_file: str = Field(
        description='The output directory for the embedded files')
    template_file: str = Field(
        description='The template file to use for embedding')


class ShaderInfo(TypedDict):
    variable_name: str
    words: list[int]


def joining(source, options, delimiter: str, elements: Collection[object]) -> list[str]:
    result = []
    i = 0
    for element in elements:
        result.append(options['fn']({
            'element': element,
            'separator': delimiter if i < len(elements) - 1 else '',
        }))
        i += 1
    return result


def get_shader_info(shader_file: str) -> ShaderInfo:
    path = Path(shader_file)
    return {
        'variable_name': path.stem.replace('.', '_'),
        'words': numpy.fromfile(shader_file, dtype=numpy.uint32).tolist(),
    }


def main(args: ProgramArgs):
    with open(args.template_file, 'r') as f:
        template_content = f.read()

    compiler = Compiler()
    template = compiler.compile(template_content)
    helpers = {
        'joining': joining,
    }
    template_args = {
        'shaders': [get_shader_info(shader_file) for shader_file in args.input_files]
    }
    output_content = template(template_args, helpers=helpers)
    with open(args.output_file, 'w') as f:
        f.write(output_content)


if __name__ == '__main__':
    main(tapify(ProgramArgs, underscores_to_dashes=True))
