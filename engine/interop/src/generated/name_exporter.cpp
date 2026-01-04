/**
 * @file name_exporter.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.interop;

import std;

namespace retro::name_exporter
{
    Name lookup(const char16_t *name, const int32 length, const FindType find_type)
    {
        return Name{std::u16string_view{name, static_cast<usize>(length)}, find_type};
    }

    bool is_valid(const Name name)
    {
        return name.is_valid();
    }

    bool equals(const Name lhs, const char16_t *rhs, const int32 rhs_len)
    {
        return lhs == std::u16string_view{rhs, static_cast<usize>(rhs_len)};
    }

    int32 to_string(const Name name, char16_t *buffer, const int32 length)
    {
        const auto as_string = name.to_string();
        const usize string_length = std::min(as_string.size(), static_cast<usize>(length));
        std::memcpy(buffer, as_string.data(), string_length);
        return string_length;
    }
} // namespace retro::name_exporter
