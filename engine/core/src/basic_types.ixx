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

    export template <typename T>
    struct Size2
    {
        T width;
        T height;

        constexpr Size2() = default;

        constexpr Size2(T width, T height) : width(width), height(height)
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