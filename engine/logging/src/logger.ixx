//
// Created by fcors on 1/2/2026.
//
module;

#include "retro/core/exports.h"

export module retro.logging:logger;

import std;
import spdlog;
import :log_level;

namespace retro
{
    std::shared_ptr<spdlog::logger>& default_logger_storage();
    RETRO_API std::shared_ptr<spdlog::logger> get_or_create_default_logger();

    export RETRO_API void init_default_console(LogLevel level = LogLevel::Info);
    export RETRO_API void set_default_logger(std::shared_ptr<spdlog::logger> logger);

    export RETRO_API void shutdown_logging();

    export RETRO_API void set_level(LogLevel level);

    export inline LogLevel log_level()
    {
        const auto logger = get_or_create_default_logger();
        return from_spd_level(logger->level());
    }

    export template <typename... Args>
    void log(const LogLevel level, std::format_string<Args...> format, Args&&... args, std::source_location loc = std::source_location::current())
    {
        const auto logger = get_or_create_default_logger();

        spdlog::source_loc source{
            loc.file_name(),
            static_cast<int>(loc.line()),
            loc.function_name()
        };

        logger->log(
            source,
            to_spd_level(level),
            format,
            std::forward<Args>(args)...);
    }
}
