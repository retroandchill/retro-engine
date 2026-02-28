/**
 * @file time_zone.cpp
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
    RETRO_API const icu::TimeZone *retro_get_unknown_time_zone()
    {
        return &icu::TimeZone::getUnknown();
    }

    RETRO_API icu::TimeZone *retro_create_default_time_zone()
    {
        return icu::TimeZone::createDefault();
    }

    RETRO_API icu::TimeZone *retro_create_time_zone(const char16_t *id, const std::int32_t id_length)
    {
        return icu::TimeZone::createTimeZone(icu::UnicodeString{id, id_length});
    }

    RETRO_API void retro_destroy_time_zone(const icu::TimeZone *time_zone)
    {
        delete time_zone;
    }

    RETRO_API std::int32_t retro_time_zone_get_canonical_id(const char16_t *id,
                                                            const std::int32_t id_length,
                                                            char16_t *buffer,
                                                            const std::int32_t length)
    {
        UErrorCode status;
        const icu::UnicodeString input_time_zone_id{id, id_length};
        icu::UnicodeString output_time_zone_id;
        icu::TimeZone::getCanonicalID(input_time_zone_id, output_time_zone_id, status);
        return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
    }

    RETRO_API std::int32_t retro_time_zone_get_id(const icu::TimeZone *time_zone,
                                                  char16_t *buffer,
                                                  const std::int32_t length)
    {
        icu::UnicodeString output_time_zone_id;
        time_zone->getID(output_time_zone_id);
        return retro::write_to_output_buffer(output_time_zone_id, std::span{buffer, static_cast<std::size_t>(length)});
    }
}
