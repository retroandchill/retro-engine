//
// Created by fcors on 12/26/2025.
//

export module retro.core:basic_types;

import std;

namespace retro
{
    template <typename T>
    concept Arithmetic = std::is_arithmetic_v<T>;

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

    export template <Arithmetic T>
    struct Size2
    {
        T width;
        T height;

        constexpr Size2() = default;

        constexpr Size2(T width, T height) : width(width), height(height)
        {
        }
    };

    export template <Arithmetic T>
    struct Vector2
    {
        T x;
        T y;

        constexpr Vector2() = default;

        constexpr Vector2(T x, T y) : x(x), y(y)
        {
        }
    };

    export using Size2i = Size2<int>;
    export using Size2f = Size2<float>;

    export using Vector2i = Vector2<int>;
    export using Vector2f = Vector2<float>;
} // namespace retro