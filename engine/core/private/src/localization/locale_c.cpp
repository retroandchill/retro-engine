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
