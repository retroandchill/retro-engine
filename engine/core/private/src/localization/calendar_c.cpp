/**
 * @file calendar_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/calendar_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;

DECLARE_OPAQUE_C_HANDLE(Retro_Calendar, icu::GregorianCalendar)
DECLARE_OPAQUE_C_HANDLE(Retro_TimeZone, icu::TimeZone)

Retro_Calendar *retro_create_calendar()
{
    UErrorCode status;
    return retro::to_c(new icu::GregorianCalendar(status));
}

void retro_destroy_calendar(Retro_Calendar *calendar)
{
    delete retro::from_c(calendar);
}

void retro_calendar_set_time_zone(Retro_Calendar *calendar, const Retro_TimeZone *time_zone)
{
    retro::from_c(calendar)->setTimeZone(*retro::from_c(time_zone));
}

void retro_calendar_set(Retro_Calendar *calendar,
                        const int32_t year,
                        const int32_t month,
                        const int32_t day_of_month,
                        const int32_t hour,
                        const int32_t minute,
                        const int32_t second)
{
    retro::from_c(calendar)->set(year, month, day_of_month, hour, minute, second);
}

double retro_calendar_get_time(const Retro_Calendar *calendar)
{
    UErrorCode status;
    return retro::from_c(calendar)->getTime(status);
}
