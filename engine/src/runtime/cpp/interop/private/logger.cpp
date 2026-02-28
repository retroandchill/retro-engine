/**
 * @file logger.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <boost/pool/pool_alloc.hpp>

import std;
import retro.logging;
import retro.core.strings.encoding;

extern "C"
{
    RETRO_API void retro_init_logger()
    {
        retro::init_logger();
    }

    RETRO_API void retro_log(const retro::LogLevel level, const char16_t *message, const std::int32_t length)
    {
        retro::get_logger().log(level, std::u16string_view(message, length));
    }

    RETRO_API void retro_log_with_source_info(const retro::LogLevel level,
                                              const char16_t *message,
                                              const std::int32_t length,
                                              const char16_t *function_name,
                                              const std::int32_t function_name_length,
                                              const char16_t *source_file,
                                              const std::int32_t source_file_length,
                                              const std::int32_t source_line)
    {
        const auto caller_name = retro::convert_string<char>(std::u16string_view(function_name, function_name_length),
                                                             boost::pool_allocator<char>{});
        const auto source_file_name = retro::convert_string<char>(std::u16string_view(source_file, source_file_length),
                                                                  boost::pool_allocator<char>{});
        retro::get_logger(caller_name, source_file_name, source_line).log(level, std::u16string_view(message, length));
    }
}
