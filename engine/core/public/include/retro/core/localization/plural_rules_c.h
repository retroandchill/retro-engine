/**
 * @file plural_rules_c.h
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
    typedef struct Retro_PluralRules Retro_PluralRules;

    RETRO_API Retro_PluralRules *retro_create_plural_rules(const Retro_Locale *locale, int32_t type);

    RETRO_API void retro_destroy_plural_rules(Retro_PluralRules *rules);

    RETRO_API int32_t retro_plural_rules_select_int32(Retro_PluralRules *rules,
                                                      int32_t number,
                                                      char16_t *buffer,
                                                      int32_t length);

    RETRO_API int32_t retro_plural_rules_select_float64(Retro_PluralRules *rules,
                                                        double number,
                                                        char16_t *buffer,
                                                        int32_t length);

#ifdef __cplusplus
}
#endif
