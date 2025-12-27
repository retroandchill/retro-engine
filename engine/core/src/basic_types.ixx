//
// Created by fcors on 12/26/2025.
//

export module retro.core:basic_types;

namespace retro
{
    export struct Color
    {
        float red;
        float green;
        float blue;
        float alpha;

        constexpr Color() = default;

        constexpr Color(float red, float green, float blue, float alpha = 1.0f)
            : red(red), green(green), blue(blue), alpha(alpha)
        {
        }
    };

    export struct Vector2
    {
        float x;
        float y;

        constexpr Vector2() = default;

        constexpr Vector2(float x, float y) : x(x), y(y)
        {
        }
    };
} // namespace retro