/**
 * @file locale_c.h
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

    RETRO_API const Retro_Locale *retro_get_default_locale();

    RETRO_API Retro_Locale *retro_create_default_locale();

    RETRO_API Retro_Locale *retro_create_locale(const char *locale);

    RETRO_API void retro_destroy_locale(Retro_Locale *locale);

    RETRO_API uint8_t retro_locale_is_bogus(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_name(Retro_Locale *locale);

    RETRO_API int32_t retro_locale_get_display_name(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API int32_t retro_locale_get_english_name(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API const char *retro_locale_get_three_letter_language_name(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_two_letter_language_name(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_region(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_script(Retro_Locale *locale);

    RETRO_API const char *retro_locale_get_variant(Retro_Locale *locale);

    RETRO_API int32_t retro_locale_get_display_language(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API int32_t retro_locale_get_display_region(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API int32_t retro_locale_get_display_script(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API int32_t retro_locale_get_display_variant(Retro_Locale *locale, char16_t *buffer, int32_t length);

    RETRO_API uint8_t retro_locale_is_right_to_left(Retro_Locale *locale);

    RETRO_API uint32_t retro_locale_get_lcid(Retro_Locale *locale);

#ifdef __cplusplus
}
#endif
