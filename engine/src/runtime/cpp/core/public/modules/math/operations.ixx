/**
 * @file operations.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

export module retro.core.math.operations;

import std;
import gcem;
import retro.core.type_traits.basic;

namespace retro
{
    export constexpr float kinda_small_number = 1.e-4f;
    export constexpr float small_number = 1.e-8f;

    export template <Numeric T>
    constexpr T clamp(T value, T min, T max) noexcept
    {
        return gcem::min(gcem::max(value, min), max);
    }

    export template <std::floating_point T>
    constexpr bool nearly_equal(T lhs, T rhs, T tolerance = small_number) noexcept
    {
        return gcem::abs(lhs - rhs) <= tolerance;
    }

    export template <std::floating_point T>
    constexpr T radians_to_degrees(const T radians) noexcept
    {
        return radians * 180.0f / std::numbers::pi_v<T>;
    }

    export template <std::floating_point T>
    constexpr T degrees_to_radians(const T degrees) noexcept
    {
        return degrees * std::numbers::pi_v<T> / 180.f;
    }

    export template <std::integral T>
    constexpr T max_pow2_factor(T n) noexcept
    {
        return n & (~n + 1);
    }
} // namespace retro
