/**
 * @file vector.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.math.vector;

import std;
import retro.core.type_traits.basic;

namespace retro
{

    export template <Numeric T, std::size_t N>
    struct Vector;

    template <Numeric T>
    struct Vector<T, 2>
    {
        using ValueType = T;
        static constexpr std::size_t SIZE = 2;

        T x{};
        T y{};

        Vector() = default;

        constexpr Vector(T x, T y) : x(x), y(y)
        {
        }

        explicit constexpr Vector(T value) : x(value), y(value)
        {
        }

        static constexpr Vector zero() noexcept
        {
            return {0, 0};
        }
        static constexpr Vector one() noexcept
        {
            return {1, 1};
        }
        static constexpr Vector right() noexcept
        {
            return {1, 0};
        }
        static constexpr Vector left() noexcept
        {
            return {-1, 0};
        }
        static constexpr Vector up() noexcept
        {
            return {0, 1};
        }
        static constexpr Vector down() noexcept
        {
            return {0, -1};
        }

        [[nodiscard]] constexpr bool operator==(const Vector &other) const = default;
    };

    export template <Numeric T>
    using Vector2 = Vector<T, 2>;

    export using Vector2i = Vector2<std::int32_t>;
    export using Vector2u = Vector2<std::uint32_t>;
    export using Vector2f = Vector2<float>;
    export using Vector2d = Vector2<double>;

    template <Numeric T>
    struct Vector<T, 3>
    {
        using ValueType = T;
        static constexpr std::size_t SIZE = 3;

        T x{};
        T y{};
        T z{};

        Vector() = default;

        constexpr Vector(T x, T y, T z) : x(x), y(y), z(z)
        {
        }

        constexpr Vector(const Vector2<T> &other, T z) : x(other.x), y(other.y), z(z)
        {
        }

        explicit constexpr Vector(T value) : x(value), y(value), z(value)
        {
        }

        static constexpr Vector zero() noexcept
        {
            return {0, 0, 0};
        }
        static constexpr Vector one() noexcept
        {
            return {1, 1, 1};
        }
        static constexpr Vector forward() noexcept
        {
            return {0, 0, 1};
        }
        static constexpr Vector back() noexcept
        {
            return {0, 0, -1};
        }
        static constexpr Vector up() noexcept
        {
            return {0, -1, 0};
        }
        static constexpr Vector down() noexcept
        {
            return {0, 1, 0};
        }
        static constexpr Vector right() noexcept
        {
            return {1, 0, 0};
        }
        static constexpr Vector left() noexcept
        {
            return {-1, 0, 0};
        }

        [[nodiscard]] constexpr bool operator==(const Vector &other) const = default;
    };

    export template <Numeric T>
    using Vector3 = Vector<T, 3>;

    export using Vector3i = Vector3<std::int32_t>;
    export using Vector3u = Vector3<std::uint32_t>;
    export using Vector3f = Vector3<float>;
    export using Vector3d = Vector3<double>;

    export template <Numeric T>
    struct Vector<T, 4>
    {
        using ValueType = T;
        static constexpr std::size_t SIZE = 4;

        T x;
        T y;
        T z;
        T w;

        Vector() = default;

        constexpr Vector(T x, T y, T z, T w) : x(x), y(y), z(z), w(w)
        {
        }

        constexpr Vector(const Vector3<T> &other, T w) : x(other.x), y(other.y), z(other.z), w(w)
        {
        }

        constexpr Vector(const Vector2<T> &other, T z, T w) : x(other.x), y(other.y), z(z), w(w)
        {
        }

        explicit constexpr Vector(T value) : x(value), y(value), z(value), w(value)
        {
        }

        static constexpr Vector zero() noexcept
        {
            return {0, 0, 0, 0};
        }
        static constexpr Vector one() noexcept
        {
            return {1, 1, 1, 1};
        }
        static constexpr Vector forward() noexcept
        {
            return {0, 0, 1, 0};
        }
        static constexpr Vector back() noexcept
        {
            return {0, 0, -1, 0};
        }
        static constexpr Vector up() noexcept
        {
            return {0, -1, 0, 0};
        }
        static constexpr Vector down() noexcept
        {
            return {0, 1, 0, 0};
        }
        static constexpr Vector right() noexcept
        {
            return {1, 0, 0, 0};
        }
        static constexpr Vector left() noexcept
        {
            return {-1, 0, 0, 0};
        }

        [[nodiscard]] constexpr bool operator==(const Vector &other) const = default;
    };

    export template <Numeric T>
    using Vector4 = Vector<T, 4>;

    export using Vector4i = Vector4<std::int32_t>;
    export using Vector4u = Vector4<std::uint32_t>;
    export using Vector4f = Vector4<float>;
    export using Vector4d = Vector4<double>;

    export template <std::size_t I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr T &get(Vector<T, 2> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr const T &get(const Vector<T, 2> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr T get(Vector<T, 2> &&vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 3)
    constexpr T &get(Vector<T, 3> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else
            return vec.z;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 3)
    constexpr const T &get(const Vector<T, 3> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else
            return vec.z;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 3)
    constexpr T get(Vector<T, 3> &&vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else
            return vec.z;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 4)
    [[nodiscard]] constexpr T &get(Vector<T, 4> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else if constexpr (I == 2)
            return vec.z;
        else
            return vec.w;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 4)
    [[nodiscard]] constexpr const T &get(const Vector<T, 4> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else if constexpr (I == 2)
            return vec.z;
        else
            return vec.w;
    }

    export template <std::size_t I, Numeric T>
        requires(I < 4)
    [[nodiscard]] constexpr T get(Vector<T, 4> &&vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else if constexpr (I == 1)
            return vec.y;
        else if constexpr (I == 2)
            return vec.z;
        else
            return vec.w;
    }

    template <Numeric T, std::size_t N, typename Op, std::size_t... Is>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make_impl(const Vector<T, N> &lhs,
                                                  const Vector<T, N> &rhs,
                                                  Op &&op,
                                                  std::index_sequence<Is...>)
    {
        return Vector<T, N>{op(get<Is>(lhs), get<Is>(rhs))...};
    }

    template <Numeric T, std::size_t N, typename Op, std::size_t... Is>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make_impl(const Vector<T, N> &lhs,
                                                  const T &rhs,
                                                  Op &&op,
                                                  std::index_sequence<Is...>)
    {
        return Vector<T, N>{op(get<Is>(lhs), rhs)...};
    }

    template <Numeric T, std::size_t N, typename Op>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make(const Vector<T, N> &lhs, const Vector<T, N> &rhs, Op &&op)
    {
        return element_wise_make_impl(lhs, rhs, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    template <Numeric T, std::size_t N, typename Op>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make(const Vector<T, N> &lhs, const T &rhs, Op &&op)
    {
        return element_wise_make_impl(lhs, rhs, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator+(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a + b; });
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator-(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a - b; });
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator*(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a * b; });
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator*(const Vector<T, N> &lhs, const T &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a * b; });
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator/(const Vector<T, N> &lhs, const T &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a / b; });
    }

    export template <Numeric T, std::size_t N>
    [[nodiscard]] constexpr Vector<T, N> operator/(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a / b; });
    }

    template <Numeric T, std::size_t N, std::invocable<T &, const T &> Op, std::size_t... Is>
    constexpr void element_wise_apply_impl(Vector<T, N> &vec,
                                           const Vector<T, N> &other,
                                           Op &&op,
                                           std::index_sequence<Is...>)
    {
        (op(get<Is>(vec), get<Is>(other)), ...);
    }

    template <Numeric T, std::size_t N, std::invocable<T &, const T &> Op, std::size_t... Is>
    constexpr void element_wise_apply_impl(Vector<T, N> &vec, const T &other, Op &&op, std::index_sequence<Is...>)
    {
        (op(get<Is>(vec), other), ...);
    }

    template <Numeric T, std::size_t N, std::invocable<T &, const T &> Op>
    constexpr void element_wise_apply(Vector<T, N> &vec, const Vector<T, N> &other, Op op)
    {
        element_wise_apply_impl(vec, other, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    template <Numeric T, std::size_t N, std::invocable<T &, const T &> Op>
    constexpr void element_wise_apply(Vector<T, N> &vec, const T &other, Op op)
    {
        element_wise_apply_impl(vec, other, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> operator+=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a += b; });
        return lhs;
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> &operator-=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a -= b; });
        return lhs;
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> &operator*=(Vector<T, N> &lhs, const T &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a *= b; });
        return lhs;
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> &operator*=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a *= b; });
        return lhs;
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> &operator/=(Vector<T, N> &lhs, const T &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a /= b; });
        return lhs;
    }

    export template <Numeric T, std::size_t N>
    constexpr Vector<T, N> &operator/=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a /= b; });
        return lhs;
    }
} // namespace retro

export template <retro::Numeric T, std::size_t N>
struct std::tuple_size<retro::Vector<T, N>> : std::integral_constant<std::size_t, N>
{
};

export template <std::size_t I, retro::Numeric T, std::size_t N>
struct std::tuple_element<I, retro::Vector<T, N>>
{
    using type = T;
};
