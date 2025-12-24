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

namespace retro::core {
    RETRO_API void delete_me() {
        // TODO: I exist to simply give the library linkage please remove me
    }
}