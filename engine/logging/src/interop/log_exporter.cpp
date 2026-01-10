/**
 * @file log_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core;
import retro.logging;

extern "C"
{
    RETRO_API void retro_log(const retro::LogLevel level, const char16_t *message, const int32 length)
    {
        retro::get_logger().log(level, std::u16string_view(message, length));
    }
}
