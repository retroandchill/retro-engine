/**
 * @file locale_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/locale_c.h"

#include "retro/core/macros.hpp"

import retro.core.c_api;
import retro.core.localization.locale;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, retro::Locale)

Retro_Locale *retro_create_locale(const char *locale)
{
    return retro::to_c(new retro::Locale(locale));
}

void retro_destroy_locale(Retro_Locale *locale)
{
    delete retro::from_c(locale);
}

uint8_t retro_locale_is_bogus(Retro_Locale *locale)
{
    return retro::from_c(locale)->is_bogus();
}

const char *retro_locale_get_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->name();
}

const char16_t *retro_locale_get_display_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->display_name().getBuffer();
}

const char16_t *retro_locale_get_english_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->english_name().getBuffer();
}

const char *retro_locale_get_three_letter_language_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->three_letter_language_name();
}

const char *retro_locale_get_two_letter_language_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->two_letter_language_name();
}

const char *retro_locale_get_region(Retro_Locale *locale)
{
    return retro::from_c(locale)->region();
}

const char *retro_locale_get_script(Retro_Locale *locale)
{
    return retro::from_c(locale)->script();
}

const char *retro_locale_get_variant(Retro_Locale *locale)
{
    return retro::from_c(locale)->variant();
}

const char16_t *retro_locale_get_display_language(Retro_Locale *locale)
{
    return retro::from_c(locale)->display_language().getBuffer();
}

const char16_t *retro_locale_get_display_region(Retro_Locale *locale)
{
    return retro::from_c(locale)->display_region().getBuffer();
}

const char16_t *retro_locale_get_display_script(Retro_Locale *locale)
{
    return retro::from_c(locale)->display_script().getBuffer();
}

const char16_t *retro_locale_get_display_variant(Retro_Locale *locale)
{
    return retro::from_c(locale)->display_variant().getBuffer();
}

uint8_t retro_locale_is_right_to_left(Retro_Locale *locale)
{
    return retro::from_c(locale)->is_right_to_left();
}

uint32_t retro_locale_get_lcid(Retro_Locale *locale)
{
    return retro::from_c(locale)->lcid();
}
