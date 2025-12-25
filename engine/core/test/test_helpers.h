//
// Created by fcors on 12/25/2025.
//
#pragma once

#include <catch2/catch_tostring.hpp>

import retro.core.strings;

using retro::core::Name;

template<>
struct Catch::StringMaker<std::u16string_view> {
    static std::string convert(const std::u16string_view value) {
        std::ostringstream oss;
        oss << "u16\"";

        for (char16_t ch : value) {
            // Printable ASCII range
            if (ch >= u' ' && ch <= u'~') {
                oss << static_cast<char>(ch);
            } else {
                // Non-ASCII -> \uXXXX (UTF-16 code unit)
                oss << "\\u"
                    << std::uppercase << std::hex
                    << std::setw(4) << std::setfill('0')
                    << static_cast<unsigned>(ch)
                    << std::dec;
            }
        }

        oss << '"';
        return oss.str();
    }
};

template<>
struct Catch::StringMaker<std::u16string> {
    static std::string convert(const std::u16string &value) {
        return StringMaker<std::u16string_view>::convert(value);
    }
};

template<>
struct Catch::StringMaker<Name> {
    static std::string convert(const Name value) {
        return StringMaker<std::u16string>::convert(value.to_string());
    }
};