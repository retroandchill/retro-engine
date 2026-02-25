/**
 * @file collator_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.core.localization.icu;

RETRO_API icu::Collator *retro_create_collator(const icu::Locale *locale)
{
    UErrorCode status;
    return icu::Collator::createInstance(*locale, status);
}

RETRO_API void retro_destroy_collator(const icu::Collator *collator)
{
    delete collator;
}

RETRO_API icu::Collator *retro_collator_clone(const icu::Collator *collator)
{
    return collator->clone();
}

RETRO_API void retro_collator_set_strength(icu::Collator *collator, std::int32_t strength)
{
    collator->setStrength(static_cast<icu::Collator::ECollationStrength>(strength));
}

RETRO_API std::int32_t retro_collator_compare(const icu::Collator *collator,
                                              const char16_t *lhs,
                                              const std::int32_t lhs_length,
                                              const char16_t *rhs,
                                              const std::int32_t rhs_length)
{
    const icu::UnicodeString lhs_str{lhs, lhs_length};
    const icu::UnicodeString rhs_str{rhs, rhs_length};
    return collator->compare(lhs_str, rhs_str);
}
