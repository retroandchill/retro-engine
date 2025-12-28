//
// Created by fcors on 12/28/2025.
//
module;

#include "retro/core/exports.h"

export module retro.core:filesystem.file_loader;

import :defines;
import std;

namespace retro
{
    export RETRO_API std::vector<std::byte> read_binary_file(const std::filesystem::path &path);
}