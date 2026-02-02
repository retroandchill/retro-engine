/**
 * @file color.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.util.color;

import std;

namespace retro
{
    export struct Color
    {
        float red;
        float green;
        float blue;
        float alpha;

        constexpr Color() = default;

        constexpr Color(const float red, const float green, const float blue, const float alpha = 1.0f)
            : red(red), green(green), blue(blue), alpha(alpha)
        {
        }

        static constexpr Color white() noexcept
        {
            return {1.0f, 1.0f, 1.0f, 1.0f};
        }
    };
} // namespace retro
