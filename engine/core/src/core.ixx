//
// Created by fcors on 12/19/2025.
//
module;

#include <retro/core/exports.h>

export module retro.core;

import std;

export using uint8 = std::uint8_t;
export using uint16 = std::uint16_t;
export using uint32 = std::uint32_t;
export using uint64 = std::uint64_t;

export using int8 = std::int8_t;
export using int16 = std::int16_t;
export using int32 = std::int32_t;
export using int64 = std::int64_t;

export using byte = std::byte;

export using usize = std::size_t;
export using isize = std::ptrdiff_t;

#ifdef _WIN32
export using nchar = wchar_t;
#else
export using nchar = char;
#endif

namespace retro {
    export template <typename... T>
    constexpr usize hash_combine(const T&... values) {
        usize seed = 0;

        auto combine_one = [&seed]<typename U>(const U& v) constexpr {
            // Get hash of this value
            const usize h = std::hash<std::decay_t<U>>{}(v);

            // Mix it into the seed (Boost-style hash_combine)
            seed ^= h + 0x9e3779b97f4a7c15 + (seed << 6) + (seed >> 2);
        };

        (combine_one(values), ...);

        return seed;
    }
}