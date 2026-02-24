/**
 * @file time_zone_c.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"

#include <stdint.h>

#ifdef __cplusplus
extern "C"
{
#endif

    typedef struct Retro_TimeZone Retro_TimeZone;

    RETRO_API const Retro_TimeZone *retro_get_unknown_time_zone();

    RETRO_API Retro_TimeZone *retro_create_default_time_zone();

    RETRO_API Retro_TimeZone *retro_create_time_zone(const char16_t *id, int32_t id_length);

    RETRO_API void retro_destroy_time_zone(Retro_TimeZone *time_zone);

    RETRO_API int32_t retro_time_zone_get_canonical_id(const char16_t *id,
                                                       int32_t id_length,
                                                       char16_t *buffer,
                                                       int32_t length);

    RETRO_API int32_t retro_time_zone_get_id(const Retro_TimeZone *time_zone, char16_t *buffer, int32_t length);

#ifdef __cplusplus
}
#endif
