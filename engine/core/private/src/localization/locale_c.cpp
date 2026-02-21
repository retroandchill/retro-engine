/**
 * @file locale_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/locale_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;
import retro.core.localization.buffers;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale)

const Retro_Locale *retro_get_default_locale()
{
    return retro::to_c(&icu::Locale::getDefault());
}

Retro_Locale *retro_create_default_locale()
{
    return retro::to_c(new icu::Locale());
}

Retro_Locale *retro_create_locale(const char *locale)
{
    return retro::to_c(new icu::Locale(locale));
}

void retro_destroy_locale(Retro_Locale *locale)
{
    delete retro::from_c(locale);
}

uint8_t retro_locale_is_bogus(Retro_Locale *locale)
{
    return retro::from_c(locale)->isBogus();
}

const char *retro_locale_get_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->getName();
}

int32_t retro_locale_get_display_name(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayName(str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

int32_t retro_locale_get_english_name(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayName(icu::Locale("en"), str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

const char *retro_locale_get_three_letter_language_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->getISO3Language();
}

const char *retro_locale_get_two_letter_language_name(Retro_Locale *locale)
{
    return retro::from_c(locale)->getLanguage();
}

const char *retro_locale_get_region(Retro_Locale *locale)
{
    return retro::from_c(locale)->getCountry();
}

const char *retro_locale_get_script(Retro_Locale *locale)
{
    return retro::from_c(locale)->getScript();
}

const char *retro_locale_get_variant(Retro_Locale *locale)
{
    return retro::from_c(locale)->getVariant();
}

int32_t retro_locale_get_display_language(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayLanguage(str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

int32_t retro_locale_get_display_region(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayCountry(str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

int32_t retro_locale_get_display_script(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayScript(str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

int32_t retro_locale_get_display_variant(Retro_Locale *locale, char16_t *buffer, int32_t length)
{
    icu::UnicodeString str;
    retro::from_c(locale)->getDisplayVariant(str);
    return retro::write_to_output_buffer(str, std::span(buffer, length));
}

uint8_t retro_locale_is_right_to_left(Retro_Locale *locale)
{
    return retro::from_c(locale)->isRightToLeft();
}

uint32_t retro_locale_get_lcid(Retro_Locale *locale)
{
    return retro::from_c(locale)->getLCID();
}
