/**
 * @file math.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:math;

import std;
import :defines;
import :concepts;

namespace retro
{
    export template <Numeric T, usize N>
    struct Vector;

    template <Numeric T>
    struct Vector<T, 2>
    {
        using ValueType = T;
        static constexpr usize SIZE = 2;

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

    export using Vector2i = Vector2<int32>;
    export using Vector2u = Vector2<uint32>;
    export using Vector2f = Vector2<float>;
    export using Vector2d = Vector2<double>;

    template <Numeric T>
    struct Vector<T, 3>
    {
        using ValueType = T;
        static constexpr usize SIZE = 3;

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

    export using Vector3i = Vector3<int32>;
    export using Vector3u = Vector3<uint32>;
    export using Vector3f = Vector3<float>;
    export using Vector3d = Vector3<double>;

    export template <Numeric T>
    struct Vector<T, 4>
    {
        using ValueType = T;
        static constexpr usize SIZE = 4;

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

    export using Vector4i = Vector4<int32>;
    export using Vector4u = Vector4<uint32>;
    export using Vector4f = Vector4<float>;
    export using Vector4d = Vector4<double>;

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

    export template <usize I, Numeric T>
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

    export template <usize I, Numeric T>
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

    export template <usize I, Numeric T>
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

namespace retro
{
    export template <Numeric T, usize Rows, usize Columns>
    class Matrix
    {
      public:
        constexpr Matrix() = default;

        [[nodiscard]] constexpr T &operator[](const usize row, const usize columns) noexcept
        {
            return data_[row + columns * Rows];
        }

        [[nodiscard]] constexpr const T &operator[](const usize row, const usize columns) const noexcept
        {
            return data_[row + columns * Rows];
        }

        template <usize OtherCols>
        [[nodiscard]] constexpr Matrix<T, Rows, OtherCols> operator*(
            const Matrix<T, Columns, OtherCols> &rhs) const noexcept
        {
            Matrix<T, Rows, OtherCols> result;
            for (usize j = 0; j < OtherCols; ++j)
            {
                for (usize k = 0; k < Columns; ++k)
                {
                    for (usize i = 0; i < Rows; ++i)
                    {
                        result(i, j) += (*this)[i, k] * rhs[k, j];
                    }
                }
            }
            return result;
        }

        static constexpr Matrix identity() noexcept
        {
            Matrix result{};

            return result;
        }

      private:
        std::array<T, Rows * Columns> data_{};
    };

    export template <Numeric T, usize N>
    class Matrix<T, N, N>
    {
      public:
        constexpr Matrix() = default;

        [[nodiscard]] constexpr T &operator[](const usize row, const usize columns) noexcept
        {
            return data_[row + columns * N];
        }

        [[nodiscard]] constexpr const T &operator[](const usize row, const usize columns) const noexcept
        {
            return data_[row + columns * N];
        }

        [[nodiscard]] constexpr Matrix operator*(const Matrix &rhs) const noexcept
        {
            Matrix result;
            for (usize j = 0; j < N; ++j)
            {
                for (usize k = 0; k < N; ++k)
                {
                    for (usize i = 0; i < N; ++i)
                    {
                        result[i, j] += (*this)[i, k] * rhs[k, j];
                    }
                }
            }
            return result;
        }

        static constexpr Matrix identity() noexcept
        {
            Matrix m;
            for (usize i = 0; i < N; ++i)
                m[i, i] = T(1);
            return m;
        }

      private:
        std::array<T, N * N> data_{};
    };

    export template <Numeric T>
    using Matrix3x3 = Matrix<T, 3, 3>;
    export using Matrix3x3f = Matrix3x3<float>;

    export template <Numeric T>
    using Matrix4x4 = Matrix<T, 4, 4>;
    export using Matrix4x4f = Matrix4x4<float>;

    export template <Numeric T>
    constexpr Matrix3x3<T> create_translation(Vector2<T> v) noexcept
    {
        auto m = Matrix3x3<T>::identity();
        m[0, 2] = v.x;
        m[1, 2] = v.y;
        return m;
    }

    export template <Numeric T>
    constexpr Matrix3x3<T> create_rotation(T angle) noexcept
    {
        auto m = Matrix3x3<T>::identity();
        T c = std::cos(angle);
        T s = std::sin(angle);
        m[0, 0] = c;
        m[0, 1] = -s;
        m[1, 0] = s;
        m[1, 1] = c;
        return m;
    }

    export template <Numeric T>
    constexpr Matrix3x3<T> create_scale(Vector2<T> v) noexcept
    {
        auto m = Matrix3x3<T>::identity();
        m[0, 0] = v.x;
        m[1, 1] = v.y;
        return m;
    }
} // namespace retro
