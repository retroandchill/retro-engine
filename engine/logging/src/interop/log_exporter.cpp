/**
 * @file log_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.logging.interop;

import std;
import retro.logging;

namespace retro::log_exporter
{
    void log(const LogLevel level, const char16_t *message, const int32 length)
    {
        get_logger().log(level, std::u16string_view(message, length));
    }
} // namespace retro::log_exporter
