/**
 * @file name.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"

#include <stdint.h> // NOLINT We want to use a C header here

#ifdef __cplusplus
extern "C"
{
#endif

    typedef struct Retro_NameId
    {
        uint32_t id;
    } Retro_NameId;

    typedef struct Retro_Name
    {
        Retro_NameId comparison_index;
        int32_t number;
#if RETRO_WITH_CASE_PRESERVING_NAME
        Retro_NameId display_index;
#endif
    } Retro_Name;

    typedef uint8_t Retro_NameCase;
    enum
    {
        Retro_NameCase_CaseSensitive,
        Retro_NameCase_IgnoreCase,
    };

    typedef uint8_t Retro_FindType;
    enum
    {
        Retro_FindType_Find,
        Retro_FindType_Add
    };

    RETRO_API Retro_Name retro_name_lookup(const char16_t *name, int32_t length, Retro_FindType find_type);
    RETRO_API bool retro_name_is_valid(Retro_Name name);
    RETRO_API int32_t retro_name_compare(Retro_Name lhs, const char16_t *rhs, int32_t length);
    RETRO_API int32_t retro_name_compare_lexical(Retro_NameId lhs_id, Retro_NameId rhs_id, Retro_NameCase name_case);
    RETRO_API int32_t retro_name_to_string(Retro_Name name, char16_t *buffer, int32_t length);

#ifdef __cplusplus
}
#endif
