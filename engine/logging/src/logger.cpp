/**
 * @file logger.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.logging;

import spdlog;

namespace retro
{
    void init_logger()
    {
        spdlog::set_default_logger(spdlog::stdout_color_mt("engine"));
        spdlog::set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%^%l%$] [%n] %v");
    }
} // namespace retro
