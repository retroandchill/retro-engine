/**
 * @file collator_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/localization/collator_c.h"

#include "retro/core/macros.hpp"

import std;
import retro.core.c_api;
import retro.core.localization.icu;

DECLARE_OPAQUE_C_HANDLE(Retro_Locale, icu::Locale);
DECLARE_OPAQUE_C_HANDLE(Retro_Collator, icu::Collator);

Retro_Collator *retro_create_collator(const Retro_Locale *locale)
{
    UErrorCode status;
    return retro::to_c(icu::Collator::createInstance(*retro::from_c(locale), status));
}

void retro_destroy_collator(Retro_Collator *collator)
{
    delete retro::from_c(collator);
}

Retro_Collator *retro_collator_clone(const Retro_Collator *collator)
{
    return retro::to_c(retro::from_c(collator)->clone());
}

void retro_collator_set_strength(Retro_Collator *collator, int32_t strength)
{
    retro::from_c(collator)->setStrength(static_cast<icu::Collator::ECollationStrength>(strength));
}

int32_t retro_collator_compare(Retro_Collator *collator,
                               const char16_t *lhs,
                               const int32_t lhs_length,
                               const char16_t *rhs,
                               const int32_t rhs_length)
{
    const auto *collator_ptr = retro::from_c(collator);
    const icu::UnicodeString lhs_str{lhs, lhs_length};
    const icu::UnicodeString rhs_str{rhs, rhs_length};
    return collator_ptr->compare(lhs_str, rhs_str);
}
