/**
 * @file align.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:memory.align;

import :defines;

namespace retro
{
    export constexpr void *align(const size_t alignment, const size_t size, void *&ptr, size_t &space)
    {
        const auto p = static_cast<std::byte *>(ptr);
        const auto off = static_cast<std::size_t>(p - static_cast<std::byte *>(nullptr)) % alignment;
        const auto adj = off == 0 ? 0 : alignment - off;

        if (space < size + adj)
        {
            return nullptr;
        }

        ptr = p + adj;
        space -= adj + size;
        return ptr;
    }
} // namespace retro
