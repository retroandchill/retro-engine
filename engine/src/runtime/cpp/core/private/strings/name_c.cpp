/**
 * @file name_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <boost/pool/pool_alloc.hpp>

import std;
import retro.core.strings.name;

namespace
{
    constexpr std::int32_t strong_ordering_to_int(const std::strong_ordering ord)
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
    RETRO_API retro::Name retro_name_lookup(const char16_t *name,
                                            const std::int32_t length,
                                            const retro::FindType find_type)
    {
        return retro::Name{std::u16string_view{name, static_cast<std::size_t>(length)}, find_type};
    }

    RETRO_API bool retro_name_is_valid(const retro::Name name)
    {
        return name.is_valid();
    }

    RETRO_API std::int32_t retro_name_compare(const retro::Name lhs, const char16_t *rhs, const std::int32_t length)
    {
        return strong_ordering_to_int(lhs <=> std::u16string_view{rhs, static_cast<std::size_t>(length)});
    }

    RETRO_API std::int32_t retro_name_compare_lexical(const retro::NameEntryId lhs_id,
                                                      const retro::NameEntryId rhs_id,
                                                      const retro::NameCase name_case)
    {
        if (name_case == retro::NameCase::case_sensitive)
        {
            return strong_ordering_to_int(lhs_id.compare_lexical_case_sensitive(rhs_id));
        }

        return strong_ordering_to_int(lhs_id.compare_lexical(rhs_id));
    }

    RETRO_API std::int32_t retro_name_to_string(const retro::Name name, char16_t *buffer, const std::int32_t length)
    {
        const auto utf16_string = name.to_string<char16_t>(boost::pool_allocator<char16_t>{});
        const std::size_t string_length = std::min(utf16_string.size(), static_cast<std::size_t>(length));
        std::memcpy(buffer, utf16_string.data(), string_length * sizeof(char16_t));
        return static_cast<std::int32_t>(string_length);
    }
}
