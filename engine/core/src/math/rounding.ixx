/**
 * @file rounding.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:math.rounding;

import std;

namespace retro
{

    export template <std::unsigned_integral T>
    constexpr T round_up_to_power_of_two(T value)
    {
        if (value <= 1)
            return 1;
        return std::bit_ceil(value);
    }
} // namespace retro
