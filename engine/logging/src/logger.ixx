/**
 * @file logger.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

#include <boost/pool/pool_alloc.hpp>

export module retro.logging:logger;

import std;
import spdlog;
import uni_algo;
import :log_level;

namespace retro
{
    template <Char T>
    auto convert_to_utf8(std::basic_string_view<T> str)
    {
        if constexpr (std::same_as<T, char16_t>)
        {
            return una::utf16to8(str, boost::pool_allocator<char>{});
        }
        else
        {
            return una::utf32to8(str, boost::pool_allocator<char>{});
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
                auto log_string = convert_to_utf8(message);
                constexpr size_t SmallBufferSize = 1024;

                if (message.size() * 4 <= SmallBufferSize)
                {
                    std::array<char, SmallBufferSize> buffer;
                    auto it = convert_to_utf8(message.begin(), message.end(), buffer.begin());
                    const usize length = std::distance(buffer.begin(), it);
                    logger_->log(loc, to_spd_level(level), std::string_view(buffer.data(), length));
                }
                else
                {
                    // Fallback to heap for unusually large strings
                    std::string long_message;
                    long_message.reserve(message.size() * 3); // Average case
                    convert_to_utf8(message.begin(), message.end(), std::back_inserter(long_message));
                    logger_->log(loc, to_spd_level(level), long_message);
                }
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

        template <typename... Args>
        void trace(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Trace, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void debug(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Debug, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void info(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Info, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void warn(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Warn, fmt, std::forward<Args>(args)...);
        }

        template <typename... Args>
        void error(const std::format_string<Args...> fmt, Args &&...args)
        {
            log(LogLevel::Error, fmt, std::forward<Args>(args)...);
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
