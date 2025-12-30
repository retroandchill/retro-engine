//
// Created by fcors on 12/29/2025.
//

export module retro.core:math.vector;

import std;
import :defines;
import :math.concepts;

namespace retro
{
    export template <Numeric T, usize N>
    struct Vector;

    template <Numeric T>
    struct Vector<T, 2>
    {
        using value_type = T;

        T x{};
        T y{};

        Vector() = default;

        constexpr Vector(T x, T y) : x(x), y(y)
        {
        }

        explicit constexpr Vector(T value) : x(value), y(value)
        {
        }

        [[nodiscard]] constexpr bool operator==(const Vector &other) const = default;
    };

    export template <Numeric T>
    using Vector2 = Vector<T, 2>;

    export using Vector2i = Vector2<int32>;
    export using Vector2u = Vector2<uint32>;
    export using Vector2f = Vector2<float>;
    export using Vector2d = Vector2<double>;

    template <Numeric T>
    struct Vector<T, 3>
    {
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

        [[nodiscard]] constexpr bool operator==(const Vector &other) const = default;
    };

    export template <Numeric T>
    using Vector3 = Vector<T, 3>;

    export using Vector3i = Vector3<int32>;
    export using Vector3u = Vector3<uint32>;
    export using Vector3f = Vector3<float>;
    export using Vector3d = Vector3<double>;

    export template <usize I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr T &get(Vector<T, 2> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <usize I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr const T &get(const Vector<T, 2> &vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <usize I, Numeric T>
        requires(I < 2)
    [[nodiscard]] constexpr T get(Vector<T, 2> &&vec) noexcept
    {
        if constexpr (I == 0)
            return vec.x;
        else
            return vec.y;
    }

    export template <usize I, Numeric T>
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

    export template <usize I, Numeric T>
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

    export template <usize I, Numeric T>
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

    template <Numeric T, usize N, typename Op, usize... Is>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make_impl(const Vector<T, N> &lhs,
                                                  const Vector<T, N> &rhs,
                                                  Op &&op,
                                                  std::index_sequence<Is...>)
    {
        return Vector<T, N>{op(get<Is>(lhs), get<Is>(rhs))...};
    }

    template <Numeric T, usize N, typename Op, usize... Is>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make_impl(const Vector<T, N> &lhs,
                                                  const T &rhs,
                                                  Op &&op,
                                                  std::index_sequence<Is...>)
    {
        return Vector<T, N>{op(get<Is>(lhs), rhs)...};
    }

    template <Numeric T, usize N, typename Op>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make(const Vector<T, N> &lhs, const Vector<T, N> &rhs, Op &&op)
    {
        return element_wise_make_impl(lhs, rhs, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    template <Numeric T, usize N, typename Op>
        requires std::is_invocable_r_v<T, Op, const T &, const T &>
    constexpr Vector<T, N> element_wise_make(const Vector<T, N> &lhs, const T &rhs, Op &&op)
    {
        return element_wise_make_impl(lhs, rhs, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    export template <Numeric T, usize N>
    [[nodiscard]] constexpr Vector<T, N> operator+(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a + b; });
    }

    export template <Numeric T, usize N>
    [[nodiscard]] constexpr Vector<T, N> operator-(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a - b; });
    }

    export template <Numeric T, usize N>
    [[nodiscard]] constexpr Vector<T, N> operator*(const Vector<T, N> &lhs, const T &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a * b; });
    }

    export template <Numeric T, usize N>
    [[nodiscard]] constexpr Vector<T, N> operator/(const Vector<T, N> &lhs, const T &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a / b; });
    }

    template <Numeric T, usize N, std::invocable<T &, const T &> Op, usize... Is>
    constexpr void element_wise_apply_impl(Vector<T, N> &vec,
                                           const Vector<T, N> &other,
                                           Op &&op,
                                           std::index_sequence<Is...>)
    {
        (op(get<Is>(vec), get<Is>(other)), ...);
    }

    template <Numeric T, usize N, std::invocable<T &, const T &> Op, usize... Is>
    constexpr void element_wise_apply_impl(Vector<T, N> &vec, const T &other, Op &&op, std::index_sequence<Is...>)
    {
        (op(get<Is>(vec), other), ...);
    }

    template <Numeric T, usize N, std::invocable<T &, const T &> Op>
    constexpr void element_wise_apply(Vector<T, N> &vec, const Vector<T, N> &other, Op op)
    {
        element_wise_apply_impl(vec, other, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    template <Numeric T, usize N, std::invocable<T &, const T &> Op>
    constexpr void element_wise_apply(Vector<T, N> &vec, const T &other, Op op)
    {
        element_wise_apply_impl(vec, other, std::forward<Op>(op), std::make_index_sequence<N>{});
    }

    export template <Numeric T, usize N>
    constexpr Vector<T, N> operator+=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a += b; });
        return lhs;
    }

    export template <Numeric T, usize N>
    constexpr Vector<T, N> &operator-=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a -= b; });
        return lhs;
    }

    export template <Numeric T, usize N>
    constexpr Vector<T, N> &operator*=(Vector<T, N> &lhs, const T &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a *= b; });
        return lhs;
    }

    export template <Numeric T, usize N>
    constexpr Vector<T, N> &operator/=(Vector<T, N> &lhs, const T &rhs)
    {
        element_wise_apply(lhs, rhs, [](T &a, const T &b) { a /= b; });
        return lhs;
    }
} // namespace retro

export template <retro::Numeric T, usize N>
struct std::tuple_size<retro::Vector<T, N>> : std::integral_constant<usize, N>
{
};

export template <usize I, retro::Numeric T, usize N>
struct std::tuple_element<I, retro::Vector<T, N>>
{
    using type = T;
};