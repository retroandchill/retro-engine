/**
 * @file name_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <cassert>

import std;
import retro.core;

static_assert(std::is_standard_layout_v<retro::Name>);

static constexpr int32 strong_ordering_to_int(const std::strong_ordering ord)
{
    if (ord < 0)
        return -1;
    if (ord > 0)
        return 1;
    return 0;
}

extern "C"
{
    RETRO_API retro::Name retro_name_lookup(const char16_t *name, const int32 length, const retro::FindType find_type)
    {
        return retro::Name{std::u16string_view{name, static_cast<usize>(length)}, find_type};
    }

    RETRO_API bool retro_name_is_valid(const retro::Name name)
    {
        return name.is_valid();
    }

    RETRO_API int32 retro_name_compare(const retro::Name lhs, const char16_t *rhs, const int32 length)
    {
        return strong_ordering_to_int(lhs <=> std::u16string_view{rhs, static_cast<usize>(length)});
    }

    RETRO_API int32 retro_name_compare_lexical(const retro::NameEntryId lhs,
                                               const retro::NameEntryId rhs,
                                               const retro::NameCase nameCase)
    {
        if (nameCase == retro::NameCase::CaseSensitive)
        {
            return strong_ordering_to_int(lhs.compare_lexical_case_sensitive(rhs));
        }

        return strong_ordering_to_int(lhs.compare_lexical(rhs));
    }

    RETRO_API int32 retro_name_to_string(const retro::Name name, char16_t *buffer, const int32 length)
    {
        const auto as_string = name.to_string();
        const usize string_length = std::min(as_string.size(), static_cast<usize>(length));
        std::memcpy(buffer, as_string.data(), string_length);
        return string_length;
    }
}
