/**
 * @file utils.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;
#include "retro/core/exports.h"

export module retro.runtime:filesystem.utils;

import std;

namespace retro::filesystem
{
    export [[nodiscard]] RETRO_API std::filesystem::path get_executable_path();
}
