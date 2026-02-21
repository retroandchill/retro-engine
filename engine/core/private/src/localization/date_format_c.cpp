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

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale)
DECLARE_OPAQUE_C_HANDLE(Retro_DecimalFormat, icu::DecimalFormat)
DECLARE_OPAQUE_C_HANDLE(Retro_DateFormat, icu::DateFormat)

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

int32_t retro_time_zone_get_canonical_id(const char16_t *id, const int32_t id_length, char16_t *buffer, int32_t length)
{
    UErrorCode status;
    const icu::UnicodeString input_time_zone_id{id, id_length};
    icu::UnicodeString output_time_zone_id;
    icu::TimeZone::getCanonicalID(input_time_zone_id, output_time_zone_id, status);
    return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
}

int32_t retro_date_format_get_time_zone_id(Retro_DateFormat *format, char16_t *buffer, int32_t length)
{
    icu::UnicodeString output_time_zone_id;
    retro::from_c(format)->getTimeZone().getID(output_time_zone_id);
    return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
}

void retro_date_format_set_time_zone(Retro_DateFormat *format, const char16_t *id, const int32_t id_length)
{
    retro::from_c(format)->adoptTimeZone(icu::TimeZone::createTimeZone(icu::UnicodeString{id, id_length}));
}

void retro_date_format_set_default_time_zone(Retro_DateFormat *format)
{
    retro::from_c(format)->adoptTimeZone(icu::TimeZone::createDefault());
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
