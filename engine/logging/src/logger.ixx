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
    export class Logger
    {
    public:
        explicit inline Logger(
        spdlog::logger* logger = spdlog::default_logger_raw(),
        std::source_location location = std::source_location::current()
    )
        : logger_(logger), location_(std::move(location))
        {
        }

        template <typename... Args>
    void log(const LogLevel level, const std::format_string<Args...> fmt, Args&&... args)
        {
            auto message = std::format(fmt, std::forward<Args>(args)...);
            const auto file_name = location_.file_name();
            const auto line = location_.line();
            const auto function_name = location_.function_name();
            spdlog::source_loc loc(file_name, line, function_name);
            logger_->log(loc, to_spd_level(level), message);
        }

        template <typename... Args>
        void trace(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Trace, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void debug(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Debug, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void info(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Info, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void warn(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Warn, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void error(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Error, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void critical(const std::format_string<Args...> fmt, Args&&... args)
        {
            log(LogLevel::Critical, fmt, std::forward<Args>(args)...);
        }

    private:
        spdlog::logger* logger_ = spdlog::default_logger_raw();
        std::source_location location_;
    };

    export RETRO_API void init_logger();

    export inline Logger get_logger(
        spdlog::logger* logger = spdlog::default_logger_raw(),
        std::source_location location = std::source_location::current()
    )
    {
        return Logger(logger, std::move(location));
    }
}
