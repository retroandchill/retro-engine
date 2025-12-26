//
// Created by fcors on 12/23/2025.
//
module;
#include "retro/core/exports.h"

export module retro.platform:filesystem.utils;

import std;

namespace retro::filesystem
{
    export [[nodiscard]] RETRO_API std::filesystem::path get_executable_path();
}