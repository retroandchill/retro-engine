/**
 * @file logging.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <spdlog/spdlog.h>

export module retro.logging;

import retro.core;
import std;

namespace retro
{
    export enum class LogLevel : uint8
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Critical,
        Off
    };

    export constexpr std::string_view to_string(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case Trace:
                return "trace";
            case Debug:
                return "debug";
            case Info:
                return "info";
            case Warn:
                return "warn";
            case Error:
                return "error";
            case Critical:
                return "critical";
            case Off:
                return "off";
        }
        return "unknown";
    }

    constexpr LogLevel from_spd_level(spdlog::level::level_enum level)
    {
        switch (level)
        {
            using enum spdlog::level::level_enum;
            case trace:
                return LogLevel::Trace;
            case debug:
                return LogLevel::Debug;
            case info:
                return LogLevel::Info;
            case warn:
                return LogLevel::Warn;
            case err:
                return LogLevel::Error;
            case critical:
                return LogLevel::Critical;
            case off:
            default:
                return LogLevel::Off;
        }
    }

    export constexpr spdlog::level::level_enum to_spd_level(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case Trace:
                return spdlog::level::trace;
            case Debug:
                return spdlog::level::debug;
            case Info:
                return spdlog::level::info;
            case Warn:
                return spdlog::level::warn;
            case Error:
                return spdlog::level::err;
            case Critical:
                return spdlog::level::critical;
            case Off:
            default:
                return spdlog::level::off;
        }
    }

    export class Logger
    {
      public:
        explicit inline Logger(spdlog::logger *logger = spdlog::default_logger_raw(),
                               std::source_location location = std::source_location::current())
            : logger_(logger), location_(std::move(location))
        {
        }

        template <Char T>
        void log(const LogLevel level, const T *message)
        {
            log(level, std::basic_string_view<T>(message));
        }

        template <Char T>
        void log(const LogLevel level, std::basic_string_view<T> message)
        {
            const auto file_name = location_.file_name();
            const auto line = location_.line();
            const auto function_name = location_.function_name();
            spdlog::source_loc loc(file_name, line, function_name);
            if constexpr (std::is_same_v<T, char>)
            {
                logger_->log(loc, to_spd_level(level), message);
            }
            else
            {
                auto log_string = convert_string<char>(message);
                logger_->log(loc, to_spd_level(level), log_string);
            }
        }

        template <typename... Args>
        void log(const LogLevel level, const std::format_string<Args...> fmt, Args &&...args)
        {
            auto message = std::format(fmt, std::forward<Args>(args)...);
            const auto file_name = location_.file_name();
            const auto line = location_.line();
            const auto function_name = location_.function_name();
            spdlog::source_loc loc(file_name, line, function_name);
            logger_->log(loc, to_spd_level(level), message);
        }

        template <Char T>
        void trace(const std::basic_string_view<T> message)
        {
            log(LogLevel::Trace, message);
        }

        template <Char T>
        void trace(const T *message)
        {
            log(LogLevel::Trace, message);
        }

        template <typename... Args>
        void trace(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Trace, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void debug(const T *message)
        {
            log(LogLevel::Debug, message);
        }

        template <Char T>
        void debug(const std::basic_string_view<T> message)
        {
            log(LogLevel::Debug, message);
        }

        template <typename... Args>
        void debug(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Debug, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void info(const T *message)
        {
            log(LogLevel::Info, message);
        }

        template <Char T>
        void info(const std::basic_string_view<T> message)
        {
            log(LogLevel::Info, message);
        }

        template <typename... Args>
        void info(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Info, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void warn(const T *message)
        {
            log(LogLevel::Warn, message);
        }

        template <Char T>
        void warn(const std::basic_string_view<T> message)
        {
            log(LogLevel::Warn, message);
        }

        template <typename... Args>
        void warn(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Warn, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void error(const T *message)
        {
            log(LogLevel::Error, message);
        }

        template <Char T>
        void error(const std::basic_string_view<T> message)
        {
            log(LogLevel::Error, message);
        }

        template <typename... Args>
        void error(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Error, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void critical(const T *message)
        {
            log(LogLevel::Critical, message);
        }

        template <Char T>
        void critical(const std::basic_string_view<T> message)
        {
            log(LogLevel::Critical, message);
        }

        template <typename... Args>
        void critical(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Critical, fmt, std::forward<Args>(args)...);
        }

      private:
        spdlog::logger *logger_ = spdlog::default_logger_raw();
        std::source_location location_;
    };

    export RETRO_API void init_logger();

    export inline Logger get_logger(spdlog::logger *logger = spdlog::default_logger_raw(),
                                    std::source_location location = std::source_location::current())
    {
        return Logger(logger, std::move(location));
    }
} // namespace retro
