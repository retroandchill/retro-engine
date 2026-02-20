/**
 * @file locale_c.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"

#include <cstdint>

#ifdef __cplusplus
extern "C"
{
#endif
    typedef struct Retro_Locale Retro_Locale;

    RETRO_API Retro_Locale *retro_create_locale(const char *locale);

    RETRO_API void retro_destroy_locale(Retro_Locale *locale);

    RETRO_API uint8_t retro_locale_is_bogus(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_name(Retro_Locale *locale);

    RETRO_API const char16_t *retro_locale_get_display_name(Retro_Locale *locale);

#ifdef __cplusplus
}
#endif
