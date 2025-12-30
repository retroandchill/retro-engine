//
// Created by fcors on 12/26/2025.
//

export module retro.core:basic_types;

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
    };
} // namespace retro