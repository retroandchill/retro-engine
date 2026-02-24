/**
 * @file calendar_c.h
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
    typedef struct Retro_Calendar Retro_Calendar;
    typedef struct Retro_TimeZone Retro_TimeZone;

    RETRO_API Retro_Calendar *retro_create_calendar();

    RETRO_API void retro_destroy_calendar(Retro_Calendar *calendar);

    RETRO_API void retro_calendar_set_time_zone(Retro_Calendar *calendar, const Retro_TimeZone *time_zone);

    RETRO_API void retro_calendar_set(Retro_Calendar *calendar,
                                      int32_t year,
                                      int32_t month,
                                      int32_t day_of_month,
                                      int32_t hour,
                                      int32_t minute,
                                      int32_t second);

    RETRO_API double retro_calendar_get_time(const Retro_Calendar *calendar);

#ifdef __cplusplus
}
#endif
