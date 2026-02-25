/**
 * @file plural_rules_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.localization.icu;
import retro.core.localization.buffers;

extern "C"
{
    RETRO_API icu::PluralRules *retro_create_plural_rules(const icu::Locale *locale, const UPluralType type)
    {
        UErrorCode status;
        return icu::PluralRules::forLocale(*locale, type, status);
    }

    RETRO_API void retro_destroy_plural_rules(const icu::PluralRules *rules)
    {
        delete rules;
    }

    RETRO_API std::int32_t retro_plural_rules_select_int32(const icu::PluralRules *rules,
                                                           const std::int32_t number,
                                                           char16_t *buffer,
                                                           const std::int32_t length)
    {
        const auto result = rules->select(number);
        return retro::write_to_output_buffer(result, std::span{buffer, static_cast<std::size_t>(length)});
    }

    RETRO_API std::int32_t retro_plural_rules_select_float64(const icu::PluralRules *rules,
                                                             const double number,
                                                             char16_t *buffer,
                                                             const std::int32_t length)
    {
        const auto result = rules->select(number);
        return retro::write_to_output_buffer(result, std::span{buffer, static_cast<std::size_t>(length)});
    }
}
