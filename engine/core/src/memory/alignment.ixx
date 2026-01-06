/**
 * @file alignment.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:memory.alignment;

import std;
import :defines;

namespace retro
{
    export template <typename T>
        requires(std::is_integral_v<T> || std::is_pointer_v<T>)
    constexpr T align(T value, const uint32 alignment)
    {
        return std::bit_cast<T>(static_cast<uint64>(value) + alignment - 1 & ~(alignment - 1));
    }

    export template <typename T>
    constexpr T read_unaligned(const void *ptr)
    {
        T aligned;
        std::memcpy(&aligned, ptr, sizeof(T));
        return aligned;
    }
} // namespace retro
