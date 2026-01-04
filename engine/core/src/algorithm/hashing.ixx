/**
 * @file hashing.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:algorithm.hashing;

import :defines;

namespace retro
{
    export template <typename... T>
    constexpr usize hash_combine(const T &...values)
    {
        usize seed = 0;

        auto combine_one = [&seed]<typename U>(const U &v) constexpr
        {
            // Get hash of this value
            const usize h = std::hash<std::decay_t<U>>{}(v);

            // Mix it into the seed (Boost-style hash_combine)
            seed ^= h + 0x9e3779b97f4a7c15 + (seed << 6) + (seed >> 2);
        };

        (combine_one(values), ...);

        return seed;
    }
} // namespace retro
