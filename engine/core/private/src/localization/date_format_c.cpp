/**
 * @file date_format_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/date_format_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;
import retro.core.localization.buffers;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale)
DECLARE_OPAQUE_C_HANDLE(Retro_DecimalFormat, icu::DecimalFormat)
DECLARE_OPAQUE_C_HANDLE(Retro_DateFormat, icu::DateFormat)
DECLARE_OPAQUE_C_HANDLE(Retro_TimeZone, icu::TimeZone)

Retro_DateFormat *retro_create_date_format(Retro_Locale *locale, int32_t date_format)
{
    return retro::to_c(
        icu::DateFormat::createDateInstance(static_cast<icu::DateFormat::EStyle>(date_format), *retro::from_c(locale)));
}

Retro_DateFormat *retro_create_time_format(Retro_Locale *locale, int32_t time_format)
{
    return retro::to_c(
        icu::DateFormat::createTimeInstance(static_cast<icu::DateFormat::EStyle>(time_format), *retro::from_c(locale)));
}

Retro_DateFormat *retro_create_date_time_format(Retro_Locale *locale, int32_t date_format, int32_t time_format)
{
    return retro::to_c(icu::DateFormat::createDateTimeInstance(static_cast<icu::DateFormat::EStyle>(date_format),
                                                               static_cast<icu::DateFormat::EStyle>(time_format),
                                                               *retro::from_c(locale)));
}

Retro_DateFormat *retro_create_custom_date_format(Retro_Locale *locale, const char16_t *pattern, int32_t pattern_length)
{
    UErrorCode status;
    return retro::to_c(icu::DateFormat::createInstanceForSkeleton(icu::UnicodeString{pattern, pattern_length},
                                                                  *retro::from_c(locale),
                                                                  status));
}

void retro_destroy_date_format(Retro_DateFormat *format)
{
    delete retro::from_c(format);
}

void retro_date_format_set_time_zone(Retro_DateFormat *format, const Retro_TimeZone *time_zone)
{
    retro::from_c(format)->setTimeZone(*retro::from_c(time_zone));
}

void retro_date_format_set_decimal_format(Retro_DateFormat *format, const Retro_DecimalFormat *decimal_format)
{
    retro::from_c(format)->setNumberFormat(*retro::from_c(decimal_format));
}

int32_t retro_date_format_format(Retro_DateFormat *format, const double date_time, char16_t *buffer, int32_t length)
{
    icu::UnicodeString output_string;
    retro::from_c(format)->format(date_time, output_string);
    return retro::write_to_output_buffer(output_string, std::span{buffer, static_cast<std::size_t>(length)});
}
