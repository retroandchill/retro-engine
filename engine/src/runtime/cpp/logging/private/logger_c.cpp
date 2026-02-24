/**
 * @file logger_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/logging/logger_c.h"

import std;
import retro.logging;

extern "C"
{
    void retro_log(const Retro_LogLevel level, const char16_t *message, const std::int32_t length)
    {
        retro::get_logger().log(static_cast<retro::LogLevel>(level), std::u16string_view(message, length));
    }
}
