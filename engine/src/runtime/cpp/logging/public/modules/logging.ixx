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

import retro.core.strings.encoding;
import retro.core.type_traits.basic;
import std;

namespace retro
{
    export enum class LogLevel : std::uint8_t
    {
        trace,
        debug,
        info,
        warn,
        error,
        critical,
        off
    };

    export constexpr std::string_view to_string(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case trace:
                return "trace";
            case debug:
                return "debug";
            case info:
                return "info";
            case warn:
                return "warn";
            case error:
                return "error";
            case critical:
                return "critical";
            case off:
                return "off";
        }
        return "unknown";
    }

    constexpr LogLevel from_spd_level(const spdlog::level::level_enum level)
    {
        switch (level)
        {
            case spdlog::level::level_enum::trace:
                return LogLevel::trace;
            case spdlog::level::level_enum::debug:
                return LogLevel::debug;
            case spdlog::level::level_enum::info:
                return LogLevel::info;
            case spdlog::level::level_enum::warn:
                return LogLevel::warn;
            case spdlog::level::level_enum::err:
                return LogLevel::error;
            case spdlog::level::level_enum::critical:
                return LogLevel::critical;
            case spdlog::level::level_enum::off:
            default:
                return LogLevel::off;
        }
    }

    export constexpr spdlog::level::level_enum to_spd_level(const LogLevel level)
    {
        switch (level)
        {
            using enum LogLevel;
            case trace:
                return spdlog::level::trace;
            case debug:
                return spdlog::level::debug;
            case info:
                return spdlog::level::info;
            case warn:
                return spdlog::level::warn;
            case error:
                return spdlog::level::err;
            case critical:
                return spdlog::level::critical;
            case off:
            default:
                return spdlog::level::off;
        }
    }

    export class Logger
    {
      public:
        explicit inline Logger(spdlog::logger *logger = spdlog::default_logger_raw(),
                               std::source_location location = std::source_location::current())
            : logger_(logger), location_(location)
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
            spdlog::source_loc loc(file_name, static_cast<std::int32_t>(line), function_name);
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
            spdlog::source_loc loc(file_name, static_cast<std::int32_t>(line), function_name);
            logger_->log(loc, to_spd_level(level), message);
        }

        template <Char T>
        void trace(const std::basic_string_view<T> message)
        {
            log(LogLevel::trace, message);
        }

        template <Char T>
        void trace(const T *message)
        {
            log(LogLevel::trace, message);
        }

        template <typename... Args>
        void trace(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::trace, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void debug(const T *message)
        {
            log(LogLevel::debug, message);
        }

        template <Char T>
        void debug(const std::basic_string_view<T> message)
        {
            log(LogLevel::debug, message);
        }

        template <typename... Args>
        void debug(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::debug, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void info(const T *message)
        {
            log(LogLevel::info, message);
        }

        template <Char T>
        void info(const std::basic_string_view<T> message)
        {
            log(LogLevel::info, message);
        }

        template <typename... Args>
        void info(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::info, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void warn(const T *message)
        {
            log(LogLevel::warn, message);
        }

        template <Char T>
        void warn(const std::basic_string_view<T> message)
        {
            log(LogLevel::warn, message);
        }

        template <typename... Args>
        void warn(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::warn, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void error(const T *message)
        {
            log(LogLevel::error, message);
        }

        template <Char T>
        void error(const std::basic_string_view<T> message)
        {
            log(LogLevel::error, message);
        }

        template <typename... Args>
        void error(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::error, fmt, std::forward<Args>(args)...);
        }

        template <Char T>
        void critical(const T *message)
        {
            log(LogLevel::critical, message);
        }

        template <Char T>
        void critical(const std::basic_string_view<T> message)
        {
            log(LogLevel::critical, message);
        }

        template <typename... Args>
        void critical(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::critical, fmt, std::forward<Args>(args)...);
        }

      private:
        spdlog::logger *logger_ = spdlog::default_logger_raw();
        std::source_location location_;
    };

    export RETRO_API void init_logger();

    export inline Logger get_logger(spdlog::logger *logger = spdlog::default_logger_raw(),
                                    std::source_location location = std::source_location::current())
    {
        return Logger(logger, location);
    }
} // namespace retro
