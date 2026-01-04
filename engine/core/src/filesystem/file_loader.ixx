/**
 * @file file_loader.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core:filesystem.file_loader;

import :defines;
import std;

namespace retro
{
    export RETRO_API std::vector<std::byte> read_binary_file(const std::filesystem::path &path);
}
