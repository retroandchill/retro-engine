/**
 * @file collator_c.h
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
    typedef struct Retro_Collator Retro_Collator;

    RETRO_API Retro_Collator *retro_create_collator(const Retro_Locale *locale);

    RETRO_API void retro_destroy_collator(Retro_Collator *collator);

    RETRO_API Retro_Collator *retro_collator_clone(const Retro_Collator *collator);

    RETRO_API void retro_collator_set_strength(Retro_Collator *collator, int32_t strength);

    RETRO_API int32_t retro_collator_compare(Retro_Collator *collator,
                                             const char16_t *lhs,
                                             int32_t lhs_length,
                                             const char16_t *rhs,
                                             int32_t rhs_length);

#ifdef __cplusplus
}
#endif
