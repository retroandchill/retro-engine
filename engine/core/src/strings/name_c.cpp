/**
 * @file name_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/strings/name.h"

#include <cassert>

import std;
import retro.core;

static_assert(sizeof(retro::Name) == sizeof(Retro_Name) && alignof(retro::Name) == alignof(Retro_Name));
static_assert(sizeof(retro::NameEntryId) == sizeof(Retro_NameId) &&
              alignof(retro::NameEntryId) == alignof(Retro_NameId));

namespace
{
    retro::Name from_c(const Retro_Name name)
    {
        return std::bit_cast<retro::Name>(name);
    }

    Retro_Name to_c(const retro::Name name)
    {
        return std::bit_cast<Retro_Name>(name);
    }

    retro::NameEntryId from_c(const Retro_NameId id)
    {
        return retro::NameEntryId{id.id};
    }

    constexpr int32 strong_ordering_to_int(const std::strong_ordering ord)
    {
        if (ord < 0)
            return -1;
        if (ord > 0)
            return 1;
        return 0;
    }
} // namespace

extern "C"
{
    Retro_Name retro_name_lookup(const char16_t *name, const int32 length, const Retro_FindType find_type)
    {
        const retro::Name name_value{std::u16string_view{name, static_cast<usize>(length)},
                                     static_cast<retro::FindType>(find_type)};
        return to_c(name_value);
    }

    bool retro_name_is_valid(const Retro_Name name)
    {
        return from_c(name).is_valid();
    }

    int32 retro_name_compare(const Retro_Name lhs, const char16_t *rhs, const int32 length)
    {
        return strong_ordering_to_int(from_c(lhs) <=> std::u16string_view{rhs, static_cast<usize>(length)});
    }

    int32 retro_name_compare_lexical(const Retro_NameId lhs, const Retro_NameId rhs, const Retro_NameCase nameCase)
    {
        if (static_cast<retro::NameCase>(nameCase) == retro::NameCase::CaseSensitive)
        {
            return strong_ordering_to_int(from_c(lhs).compare_lexical_case_sensitive(from_c(rhs)));
        }

        return strong_ordering_to_int(from_c(lhs).compare_lexical(from_c(rhs)));
    }

    int32 retro_name_to_string(const Retro_Name name, char16_t *buffer, const int32 length)
    {
        const auto as_string = from_c(name).to_string();
        const usize string_length = std::min(as_string.size(), static_cast<usize>(length));
        std::memcpy(buffer, as_string.data(), string_length);
        return string_length;
    }
}
