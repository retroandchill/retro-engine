/**
 * @file calendar.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.localization.icu;

extern "C"
{
    RETRO_API icu::GregorianCalendar *retro_create_calendar()
    {
        UErrorCode status;
        return new icu::GregorianCalendar(status);
    }

    RETRO_API void retro_destroy_calendar(const icu::GregorianCalendar *calendar)
    {
        delete calendar;
    }

    RETRO_API void retro_calendar_set_time_zone(icu::GregorianCalendar *calendar, const icu::TimeZone *time_zone)
    {
        calendar->setTimeZone(*time_zone);
    }

    RETRO_API void retro_calendar_set(icu::GregorianCalendar *calendar,
                                      const std::int32_t year,
                                      const std::int32_t month,
                                      const std::int32_t day_of_month,
                                      const std::int32_t hour,
                                      const std::int32_t minute,
                                      const std::int32_t second)
    {
        calendar->set(year, month, day_of_month, hour, minute, second);
    }

    RETRO_API double retro_calendar_get_time(const icu::GregorianCalendar *calendar)
    {
        UErrorCode status;
        return calendar->getTime(status);
    }
}
