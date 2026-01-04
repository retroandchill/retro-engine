import os
import re
import fnmatch
from datetime import datetime

# Configuration
EXTENSIONS_CPP = {'.cpp', '.h', '.hpp', '.ixx'}
EXTENSIONS_CS = {'.cs'}
EXTENSIONS_HASH = {'.py', '.cmake'}
YEAR = datetime.now().year
AUTHOR = "Retro & Chill"

OLD_HEADER_PATTERN = re.compile(
    r'^//\s*\n// Created by .* on .*\.\n//\s*\n', re.MULTILINE)


def load_gitignore():
    patterns = []
    if os.path.exists('.gitignore'):
        with open('.gitignore', 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    # Handle basic ! negation by skipping for this simple script
                    if not line.startswith('!'):
                        patterns.append(line)
    # Always skip git metadata
    patterns.append('.git/')
    return patterns


def is_ignored(path, patterns):
    for pattern in patterns:
        # Normalize pattern for directory matching
        if pattern.endswith('/'):
            if fnmatch.fnmatch(path, pattern.rstrip('/')) or fnmatch.fnmatch(path, f"{pattern.rstrip('/')}/*"):
                return True
        elif fnmatch.fnmatch(path, pattern):
            return True
    return False


def get_header(filename, extension):
    if extension in EXTENSIONS_CPP:
        return f"""/**
 * @file {filename}
 *
 * @copyright Copyright (c) {YEAR} {AUTHOR}. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
"""
    elif extension in EXTENSIONS_CS:
        return f"""// @file ${filename}.cs
//
// @copyright Copyright (c) {YEAR} {AUTHOR}. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
"""
    elif extension in EXTENSIONS_HASH or filename == "CMakeLists.txt":
        return f"""# @file {filename}
#
# @copyright Copyright (c) {YEAR} {AUTHOR}. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
"""
    return None


def update_file(filepath):
    filename = os.path.basename(filepath)
    _, extension = os.path.splitext(filepath)

    # Handle CMakeLists.txt which has no extension
    if filename == "CMakeLists.txt":
        extension = ".cmake"

    header = get_header(filename, extension)
    if not header:
        return False

    # Use 'utf-8-sig' to handle the UTF-8 BOM commonly found in C# files
    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except Exception:
        return False

    # Check if the file already has the new header style to avoid double-processing
    if " @file " in content:
        return False

    # For Python/CMake, skip the shebang if it exists
    if OLD_HEADER_PATTERN.search(content):
        new_content = OLD_HEADER_PATTERN.sub(header, content)
    elif content.startswith("#!"):
        lines = content.splitlines(keepends=True)
        new_content = lines[0] + header + "".join(lines[1:])
    else:
        new_content = header + content

    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.write(new_content)
    return True


def main():
    ignore_patterns = load_gitignore()
    count = 0

    for root, dirs, files in os.walk('.'):
        # Remove the leading './' if present for better matching
        rel_root = os.path.relpath(root, '.')
        if rel_root == '.':
            rel_root = ''

        # Filter directories in-place
        dirs[:] = [d for d in dirs if not is_ignored(
            os.path.join(rel_root, d), ignore_patterns)]

        for file in files:
            rel_path = os.path.join(rel_root, file)
            if is_ignored(rel_path, ignore_patterns):
                continue

            if update_file(rel_path):
                print(f"Updated: {rel_path}")
                count += 1
    print(f"\nFinished! Updated {count} files.")


if __name__ == "__main__":
    main()
