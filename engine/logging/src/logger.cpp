//
// Created by fcors on 1/2/2026.
//
module retro.logging;

import spdlog;

namespace retro
{
    void init_logger()
    {
        spdlog::set_default_logger(spdlog::stdout_color_mt("engine"));
        spdlog::set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%^%l%$] [%n] %v");
    }
}
