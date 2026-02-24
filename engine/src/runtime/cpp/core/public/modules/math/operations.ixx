/**
 * @file operations.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <gcem.hpp>

export module retro.core.math.operations;

import std;
import retro.core.type_traits.basic;

namespace retro
{
    export constexpr float kinda_small_number = 1.e-4f;
    export constexpr float small_number = 1.e-8f;

    export template <Numeric T>
    constexpr T clamp(T value, T min, T max) noexcept
    {
        return std::min(std::max(value, min), max);
    }

    export template <Numeric T>
    constexpr T abs(T value) noexcept
    {
        return gcem::abs(value);
    }

    export template <std::floating_point T>
    constexpr bool nearly_equal(T lhs, T rhs, T tolerance = small_number) noexcept
    {
        return abs(lhs - rhs) <= tolerance;
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

    export template <std::floating_point T>
    constexpr T sin(const T value) noexcept
    {
        return gcem::sin(value);
    }

    export template <std::floating_point T>
    constexpr T cos(const T value) noexcept
    {
        return gcem::cos(value);
    }

    export template <std::floating_point T>
    constexpr T tan(const T value) noexcept
    {
        return gcem::tan(value);
    }

    export template <std::integral T>
    constexpr T max_pow2_factor(T n) noexcept
    {
        return n & (~n + 1);
    }
} // namespace retro
