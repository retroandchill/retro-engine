/**
 * @file plural_rules_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/plural_rules_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;
import retro.core.localization.buffers;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale);
DECLARE_OPAQUE_C_HANDLE(Retro_PluralRules, icu::PluralRules);

Retro_PluralRules *retro_create_plural_rules(const Retro_Locale *locale, int32_t type)
{
    UErrorCode status;
    return retro::to_c(icu::PluralRules::forLocale(*retro::from_c(locale), static_cast<UPluralType>(type), status));
}

void retro_destroy_plural_rules(Retro_PluralRules *rules)
{
    delete retro::from_c(rules);
}

int32_t retro_plural_rules_select_int32(Retro_PluralRules *rules,
                                        const int32_t number,
                                        char16_t *buffer,
                                        const int32_t length)
{
    const auto result = retro::from_c(rules)->select(number);
    return retro::write_to_output_buffer(result, std::span{buffer, static_cast<std::size_t>(length)});
}

int32_t retro_plural_rules_select_float64(Retro_PluralRules *rules,
                                          const double number,
                                          char16_t *buffer,
                                          const int32_t length)
{
    const auto result = retro::from_c(rules)->select(number);
    return retro::write_to_output_buffer(result, std::span{buffer, static_cast<std::size_t>(length)});
}
