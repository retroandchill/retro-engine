/**
 * @file logging.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>

module retro.logging;

namespace retro
{
    void init_logger()
    {
        spdlog::set_default_logger(spdlog::stdout_color_mt("engine"));
        spdlog::set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%^%l%$] [%n] %v");
    }
} // namespace retro
