/**
 * @file date_format_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.localization.icu;
import retro.core.localization.buffers;

extern "C"
{
    RETRO_API icu::DateFormat *retro_create_date_format(const icu::Locale *locale,
                                                        const icu::DateFormat::EStyle date_format)
    {
        return icu::DateFormat::createDateInstance(date_format, *locale);
    }

    RETRO_API icu::DateFormat *retro_create_time_format(const icu::Locale *locale,
                                                        const icu::DateFormat::EStyle time_format)
    {
        return icu::DateFormat::createTimeInstance(time_format, *locale);
    }

    RETRO_API icu::DateFormat *retro_create_date_time_format(const icu::Locale *locale,
                                                             const icu::DateFormat::EStyle date_format,
                                                             const icu::DateFormat::EStyle time_format)
    {
        return icu::DateFormat::createDateTimeInstance(date_format, time_format, *locale);
    }

    RETRO_API icu::DateFormat *retro_create_custom_date_format(const icu::Locale *locale,
                                                               const char16_t *pattern,
                                                               const std::int32_t pattern_length)
    {
        UErrorCode status;
        return icu::DateFormat::createInstanceForSkeleton(icu::UnicodeString{pattern, pattern_length}, *locale, status);
    }

    RETRO_API void retro_destroy_date_format(const icu::DateFormat *format)
    {
        delete format;
    }

    RETRO_API void retro_date_format_set_time_zone(icu::DateFormat *format, const icu::TimeZone *time_zone)
    {
        format->setTimeZone(*time_zone);
    }

    RETRO_API void retro_date_format_set_decimal_format(icu::DateFormat *format,
                                                        const icu::DecimalFormat *decimal_format)
    {
        format->setNumberFormat(*decimal_format);
    }

    RETRO_API std::int32_t retro_date_format_format(const icu::DateFormat *format,
                                                    const double date_time,
                                                    char16_t *buffer,
                                                    const std::int32_t length)
    {
        icu::UnicodeString output_string;
        format->format(date_time, output_string);
        return retro::write_to_output_buffer(output_string, std::span{buffer, static_cast<std::size_t>(length)});
    }
}
