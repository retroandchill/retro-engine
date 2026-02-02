/**
 * @file gtest_helpers.hpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include <gtest/gtest.h>

import std;
import retro.core.strings.name;

inline void PrintTo(std::u16string_view value, std::ostream *os)
{
    if (os == nullptr)
        return;

    *os << "u16\"";
    for (char16_t ch : value)
    {
        // Printable ASCII range
        if (ch >= u' ' && ch <= u'~')
        {
            *os << static_cast<char>(ch);
        }
        else
        {
            // Non-ASCII -> \uXXXX (UTF-16 code unit)
            *os << "\\u" << std::uppercase << std::hex << std::setw(4) << std::setfill('0')
                << static_cast<unsigned>(static_cast<std::uint16_t>(ch)) << std::nouppercase << std::dec;
        }
    }
    *os << "\"";
}

inline void PrintTo(const std::u16string &value, std::ostream *os)
{
    PrintTo(std::u16string_view{value}, os);
}

// Print retro::Name using its string representation.
inline void PrintTo(const retro::Name value, std::ostream *os)
{
    if (os == nullptr)
        return;
    *os << value.to_string();
}
