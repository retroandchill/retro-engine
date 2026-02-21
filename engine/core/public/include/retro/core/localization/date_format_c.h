/**
 * @file date_format_c.h
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
    typedef struct Retro_Locale Retro_Locale;
    typedef struct Retro_DecimalFormat Retro_DecimalFormat;
    typedef struct Retro_DateFormat Retro_DateFormat;

    RETRO_API Retro_DateFormat *retro_create_date_format(Retro_Locale *locale, int32_t date_format);

    RETRO_API Retro_DateFormat *retro_create_time_format(Retro_Locale *locale, int32_t time_format);

    RETRO_API Retro_DateFormat *retro_create_date_time_format(Retro_Locale *locale,
                                                              int32_t date_format,
                                                              int32_t time_format);

    RETRO_API Retro_DateFormat *retro_create_custom_date_format(Retro_Locale *locale,
                                                                const char16_t *pattern,
                                                                int32_t pattern_length);

    RETRO_API void retro_destroy_date_format(Retro_DateFormat *format);

    RETRO_API int32_t retro_time_zone_get_canonical_id(const char16_t *id,
                                                       int32_t id_length,
                                                       char16_t *buffer,
                                                       int32_t length);

    RETRO_API int32_t retro_date_format_get_time_zone_id(Retro_DateFormat *format, char16_t *buffer, int32_t length);

    RETRO_API void retro_date_format_set_time_zone(Retro_DateFormat *format, const char16_t *id, int32_t id_length);

    RETRO_API void retro_date_format_set_default_time_zone(Retro_DateFormat *format);

    RETRO_API void retro_date_format_set_decimal_format(Retro_DateFormat *format,
                                                        const Retro_DecimalFormat *decimal_format);

    RETRO_API int32_t retro_date_format_format(Retro_DateFormat *format,
                                               double date_time,
                                               char16_t *buffer,
                                               int32_t length);

#ifdef __cplusplus
}
#endif
