/**
 * @file time_zone_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/time_zone_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;
import retro.core.localization.buffers;

DECLARE_OPAQUE_C_HANDLE(Retro_TimeZone, icu::TimeZone)

const Retro_TimeZone *retro_get_unknown_time_zone()
{
    return retro::to_c(&icu::TimeZone::getUnknown());
}

Retro_TimeZone *retro_create_default_time_zone()
{
    return retro::to_c(icu::TimeZone::createDefault());
}

Retro_TimeZone *retro_create_time_zone(const char16_t *id, int32_t id_length)
{
    return retro::to_c(icu::TimeZone::createTimeZone(icu::UnicodeString{id, id_length}));
}

void retro_destroy_time_zone(Retro_TimeZone *time_zone)
{
    delete retro::from_c(time_zone);
}

int32_t retro_time_zone_get_canonical_id(const char16_t *id, const int32_t id_length, char16_t *buffer, int32_t length)
{
    UErrorCode status;
    const icu::UnicodeString input_time_zone_id{id, id_length};
    icu::UnicodeString output_time_zone_id;
    icu::TimeZone::getCanonicalID(input_time_zone_id, output_time_zone_id, status);
    return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
}

int32_t retro_time_zone_get_id(const Retro_TimeZone *time_zone, char16_t *buffer, const int32_t length)
{
    icu::UnicodeString output_time_zone_id;
    retro::from_c(time_zone)->getID(output_time_zone_id);
    return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
}
