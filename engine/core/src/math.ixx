/**
 * @file math.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <gcem.hpp>

export module retro.core:math;

import std;
import :defines;
import :concepts;

namespace retro
{
    static constexpr float PI = 3.1415926535897932384626433832795f;
    static constexpr float KINDA_SMALL_NUMBER = 1.e-4f;
    static constexpr float SMALL_NUMBER = 1.e-8f;

    static constexpr double PI_D = 3.141592653589793238462643383279502884197169399;

    template <Numeric T>
    constexpr T clamp(T value, T min, T max) noexcept
    {
        return std::min(std::max(value, min), max);
    }

    template <std::floating_point T>
    constexpr bool nearly_equal(T lhs, T rhs, T tolerance = SMALL_NUMBER) noexcept
    {
        return gcem::abs(lhs - rhs) <= tolerance;
    }

    constexpr float radians_to_degrees(const float radians) noexcept
    {
        return radians * 180.f / PI;
    }

    constexpr double radians_to_degrees(const double radians) noexcept
    {
        return radians * 180.0 / PI_D;
    }

    constexpr float degrees_to_radians(const float degrees) noexcept
    {
        return degrees * PI / 180.f;
    }

    constexpr double degrees_to_radians(const double degrees) noexcept
    {
        return degrees * PI_D / 180.0;
    }

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
    [[nodiscard]] constexpr Vector<T, N> operator*(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
    {
        return element_wise_make(lhs, rhs, [](const T &a, const T &b) { return a * b; });
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

    export template <Numeric T, usize N>
    [[nodiscard]] constexpr Vector<T, N> operator/(const Vector<T, N> &lhs, const Vector<T, N> &rhs)
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
    constexpr Vector<T, N> &operator*=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
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

    export template <Numeric T, usize N>
    constexpr Vector<T, N> &operator/=(Vector<T, N> &lhs, const Vector<T, N> &rhs)
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
    class Matrix;

    export template <Numeric T, usize N>
    class Scale
    {

      public:
        constexpr Scale() = default;

        explicit constexpr Scale(T uniform) : vec_{uniform}
        {
        }

        template <std::convertible_to<T>... Args>
            requires(sizeof...(Args) == N)
        explicit constexpr Scale(Args... args) : vec_{args...}
        {
        }

        explicit constexpr Scale(const Vector<T, N> &vec) : vec_{vec}
        {
        }

        constexpr const Vector<T, N> &vector() const
        {
            return vec_;
        }

        constexpr Vector<T, N> transform_point(const Vector<T, N> &point) const
        {
            return vec_ * point;
        }

        constexpr Vector<T, N> concatenate(const Scale &other) const
        {
            return vec_ * other.vec_;
        }

        constexpr Scale inverse() const
        {
            return Scale{1 / vec_};
        }

        constexpr friend bool operator==(const Scale &lhs, const Scale &rhs) = default;

      private:
        Vector<T, N> vec_{1, 1};
    };

    export template <Numeric T>
    using Scale2 = Scale<T, 2>;
    export using Scale2f = Scale2<float>;

    export template <Numeric T, usize N>
    class Shear;

    export template <Numeric T>
    class Shear<T, 2>
    {
      public:
        constexpr Shear() = default;

        constexpr Shear(T shear_x, T shear_y) : shear_{shear_x, shear_y}
        {
        }

        constexpr explicit Shear(const Vector2<T> &vec) : shear_{vec}
        {
        }

        constexpr const Vector2<T> &vector() const
        {
            return shear_;
        }

        static constexpr Shear from_shear_angles(const Vector2<T> &angles)
        {
            T shear_x = angles.x == 0 ? 0 : 1.0f / gcem::tan(degrees_to_radians(90 - clamp(angles.x, -89.f, 89.f)));
            T shear_y = angles.x == 0 ? 0 : 1.0f / gcem::tan(degrees_to_radians(90 - clamp(angles.u, -89.f, 89.f)));
            return Shear{shear_x, shear_y};
        }

        constexpr Vector2<T> transform_point(const Vector2<T> &point) const
        {
            return point + point * shear_;
        }

        constexpr Matrix<T, 2, 2> concatenate(const Shear &other) const
        {
            return Matrix<T, 2, 2>{1 + shear_.y + other.shear_.x,
                                   other.shear_.y * shear_.y,
                                   shear_.x + other.shear_.x,
                                   shear_.x * other.shear_.x + 1};
        }

        constexpr Matrix<T, 2, 2> inverse() const
        {
            T inv_det = 1.0f / (1.0f - shear_.x * shear_.y);
            return Matrix<T, 2, 2>{inv_det, -shear_.y * inv_det, -shear_.x * inv_det, inv_det};
        }

        constexpr friend bool operator==(const Shear &lhs, const Shear &rhs) = default;

      private:
        Vector2<T> shear_{0, 0};
    };

    export template <Numeric T>
    using Shear2 = Shear<T, 2>;
    export using Shear2f = Shear2<float>;

    export template <Numeric T, usize N>
    class Quaternion;

    export template <Numeric T>
    class Quaternion<T, 2>
    {
      public:
        constexpr Quaternion() = default;

        explicit constexpr Quaternion(T radians) : rotation_{gcem::cos(radians), gcem::sin(radians)}
        {
        }

        explicit constexpr Quaternion(const Vector2<T> &rotation) : rotation_{rotation}
        {
        }

        constexpr const Vector2<T> &vector() const
        {
            return rotation_;
        }

        constexpr Vector2<T> transform_point(const Vector2<T> &point) const
        {
            return Vector2<T>{point.x * rotation_.x - point.y * rotation_.y,
                              point.x * rotation_.y + point.y * rotation_.x};
        }

        constexpr Quaternion concatenate(const Quaternion &other) const
        {
            return Quaternion{transform_point(other.rotation_)};
        }

        constexpr Quaternion inverse() const
        {
            return Quaternion{rotation_.x, -rotation_.y};
        }

        constexpr friend bool operator==(const Quaternion &lhs, const Quaternion &rhs) = default;

      private:
        Vector2<T> rotation_{1, 0};
    };

    export template <Numeric T>
    using Quaternion2 = Quaternion<T, 2>;
    export using Quaternion2f = Quaternion2<float>;

    template <Numeric T, usize Rows, usize Columns>
    class Matrix
    {
      public:
        constexpr Matrix() = default;

        template <std::convertible_to<T>... Args>
            requires(sizeof...(Args) == Rows * Columns)
        constexpr explicit Matrix(Args... args) : data_{args...}
        {
        }

        explicit constexpr Matrix(T diagonal)
            requires(Rows == Columns)
        {
            for (usize i = 0; i < Rows; ++i)
                data_[i * Rows + i] = diagonal;
        }

        explicit constexpr Matrix(const Scale<T, Rows> &scale)
            requires(Rows == Columns)
        {
            fill_diagonal_from_vector(scale.vector(), std::make_index_sequence<Rows>{});
        }

        explicit constexpr Matrix(const Shear<T, Rows> &shear)
            requires(Rows == Columns)
        {
            fill_diagonal_from_vector_and_ones(shear.vector(), std::make_index_sequence<Rows>{});
        }

        explicit constexpr Matrix(const Quaternion<T, 2> &rotation)
            requires(Rows == Columns && Rows == 2)
        {
            T cos = rotation.vector().x;
            T sin = rotation.vector().y;
            data_[0] = cos;
            data_[1] = sin;
            data_[2] = -sin;
            data_[3] = cos;
        }

        [[nodiscard]] constexpr T &operator[](const usize row, const usize columns) noexcept
        {
            return data_[row + columns * Rows];
        }

        [[nodiscard]] constexpr const T &operator[](const usize row, const usize columns) const noexcept
        {
            return data_[row + columns * Rows];
        }

        [[nodiscard]] constexpr Matrix operator+(const Matrix &rhs) const noexcept
        {
            Matrix result;
            for (usize i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] + rhs.data_[i];
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator+=(const Matrix &rhs) const noexcept
        {
            for (usize i = 0; i < Rows * Columns; ++i)
                data_[i] += rhs.data_[i];
            return *this;
        }

        [[nodiscard]] constexpr Matrix operator-(const Matrix &rhs) const noexcept
        {
            Matrix result;
            for (usize i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] - rhs.data_[i];
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator-=(const Matrix &rhs) const noexcept
        {

            for (usize i = 0; i < Rows * Columns; ++i)
                data_[i] -= rhs.data_[i];
            return *this;
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
                        result[i, j] += (*this)[i, k] * rhs[k, j];
                    }
                }
            }
            return result;
        }

        [[nodiscard]] constexpr friend Vector<T, Columns> operator*(const Matrix &mat,
                                                                    const Vector<T, Columns> &vec) noexcept
        {
            return from_column_matrix(mat * to_column_matrix(vec));
        }

        [[nodiscard]] constexpr friend Vector<T, Rows> operator*(const Vector<T, Rows> &vec, const Matrix &mat) noexcept
        {
            return from_row_matrix(to_row_matrix(vec) * mat);
        }

        [[nodiscard]] constexpr Matrix operator*(const T &rhs) const noexcept
        {
            Matrix result;
            for (usize i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] * rhs;
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator*=(const T &rhs) const noexcept
        {
            for (usize i = 0; i < Rows * Columns; ++i)
                data_[i] *= rhs;
            return *this;
        }

        [[nodiscard]] constexpr Matrix &operator*=(const Matrix &rhs) noexcept
            requires(Rows == Columns)
        {
            for (usize j = 0; j < Columns; ++j)
            {
                for (usize k = 0; k < Columns; ++k)
                {
                    for (usize i = 0; i < Rows; ++i)
                    {
                        (*this)[i, k](i, j) += (*this)[i, k] * rhs[k, j];
                    }
                }
            }
            return *this;
        }

        [[nodiscard]] constexpr Matrix operator/(const T &rhs) const noexcept
        {
            Matrix result;
            for (usize i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] / rhs;
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator/=(const T &rhs) const noexcept
        {
            for (usize i = 0; i < Rows * Columns; ++i)
                data_[i] /= rhs;
            return *this;
        }

        static constexpr Matrix identity() noexcept
            requires(Rows == Columns)
        {
            Matrix m;
            for (usize i = 0; i < Rows; ++i)
                m[i, i] = T(1);
            return m;
        }

        [[nodiscard]] constexpr bool is_identity() const noexcept
            requires(Rows == Columns)
        {
            return *this == identity();
        }

        constexpr friend bool operator==(const Matrix &lhs, const Matrix &rhs);

      private:
        template <usize... I>
        void fill_diagonal_from_vector(const Vector<T, Rows> &diagonal, std::index_sequence<I...>)
            requires(Rows == Columns)
        {
            ((data_[I * Rows + I] = get<I>(diagonal)), ...);
        }

        template <usize... I>
        void fill_diagonal_from_vector_and_ones(const Vector<T, Rows> &diagonal, std::index_sequence<I...>)
            requires(Rows == Columns)
        {
            (
                [&]<usize Idx>
                {
                    constexpr usize row = Idx % Rows;
                    constexpr usize col = Idx / Columns;

                    if constexpr (row == col)
                        data_[Idx] = get<row>(diagonal);
                    else
                        data_[Idx] = T(1);
                }.template operator()<I>(),
                ...);
        }

        std::array<T, Rows * Columns> data_{};
    };

    template <Numeric T, usize N, usize... I>
    Matrix<T, 1, N> to_row_matrix(const Vector<T, N> &vec, std::index_sequence<I...>)
    {
        return Matrix<T, 1, N>{get<I>(vec)...};
    }

    export template <Numeric T, usize N>
    Matrix<T, 1, N> to_row_matrix(const Vector<T, N> &vec)
    {
        return to_row_matrix(vec, std::make_index_sequence<N>{});
    }

    template <Numeric T, usize N, usize... I>
    Vector<T, N> from_row_matrix(Matrix<T, 1, N> matrix, std::index_sequence<I...>)
    {
        return Vector<T, N>{matrix[0, I]...};
    }

    export template <Numeric T, usize N>
    Vector<T, N> from_row_matrix(Matrix<T, 1, N> matrix)
    {
        return from_row_matrix(matrix, std::make_index_sequence<N>{});
    }

    template <Numeric T, usize N, usize... I>
    Matrix<T, N, 1> to_column_matrix(const Vector<T, N> &vec, std::index_sequence<I...>)
    {
        return Matrix<T, N, 1>{get<I>(vec)...};
    }

    export template <Numeric T, usize N>
    Matrix<T, N, 1> to_column_matrix(const Vector<T, N> &vec)
    {
        return to_column_matrix(vec, std::make_index_sequence<N>{});
    }

    template <Numeric T, usize N, usize... I>
    Vector<T, N> from_column_matrix(Matrix<T, N, 1> matrix, std::index_sequence<I...>)
    {
        return Vector<T, N>{matrix[I, 0]...};
    }

    export template <Numeric T, usize N>
    Vector<T, N> from_column_matrix(Matrix<T, N, 1> matrix)
    {
        return from_column_matrix(matrix, std::make_index_sequence<N>{});
    }

    template <typename T, usize N, usize M>
    constexpr bool operator==(const Matrix<T, N, M> &lhs, const Matrix<T, N, M> &rhs)
    {
        if constexpr (std::is_floating_point_v<T>)
        {
            for (usize i = 0; i < N * M; ++i)
            {
                if (!nearly_equal(lhs.data_[i], rhs.data_[i], KINDA_SMALL_NUMBER))
                    return false;
            }
        }
        else
        {
            for (usize i = 0; i < N * M; ++i)
            {
                if (lhs.data_[i] != rhs.data_[i])
                    return false;
            }
        }

        return true;
    }

    template <Numeric T>
    using Matrix2x2 = Matrix<T, 2, 2>;
    export using Matrix2x2f = Matrix2x2<float>;

    export template <Numeric T>
    using Matrix3x3 = Matrix<T, 3, 3>;
    export using Matrix3x3f = Matrix3x3<float>;

    export template <Numeric T>
    using Matrix4x4 = Matrix<T, 4, 4>;
    export using Matrix4x4f = Matrix4x4<float>;

    export template <std::floating_point T, usize N>
    class Transform;

    export template <std::floating_point T>
    class Transform<T, 2>
    {
      public:
        constexpr explicit Transform(const Vector2<T> &position = {}) : position_(position)
        {
        }

        constexpr explicit Transform(T uniformScale, const Vector2<T> &translation = {})
            : matrix_(Scale2<T>(uniformScale)), position_(translation)
        {
        }

        constexpr explicit Transform(const Scale2<T> &scale, const Vector2<T> &translation = {})
            : matrix_(scale), position_(translation)
        {
        }

        constexpr explicit Transform(const Shear2<T> &shear, const Vector2<T> &translation = {})
            : matrix_(shear), position_(translation)
        {
        }

        constexpr explicit Transform(const Quaternion2<T> &rotation, const Vector2<T> &translation = {})
            : matrix_(rotation), position_(translation)
        {
        }

        constexpr explicit Transform(const Matrix2x2<T> &matrix, const Vector2<T> &translation = {})
            : matrix_(matrix), position_(translation)
        {
        }

        constexpr const Matrix2x2<T> &matrix() const
        {
            return matrix_;
        }

        constexpr const Vector2<T> &translation() const
        {
            return position_;
        }

        constexpr void set_translation(const Vector2<T> &translation)
        {
            position_ = translation;
        }

        constexpr Vector2<T> transform_point(const Vector2<T> &point) const
        {
            return position_ * (matrix_ * point);
        }

        constexpr Vector2<T> transform_vector(const Vector2<T> &vector) const
        {
            return matrix_ * vector;
        }

        constexpr Transform concatenate(const Transform &other) const
        {
            return Transform{matrix_ * other.matrix_, (position_ * other.matrix_) + other.position_};
        }

        constexpr Transform inverse() const
        {
            return Transform{matrix_.inverse(), -position_};
        }

        constexpr friend bool operator==(const Transform &lhs, const Transform &rhs) = default;

        [[nodiscard]] constexpr bool is_identity() const
        {
            return matrix_.is_identity() && position_ == Vector2<T>::zero();
        }

      private:
        Matrix2x2<T> matrix_;
        Vector2<T> position_;
    };

    export template <std::floating_point T>
    using Transform2 = Transform<T, 2>;

    export using Transform2f = Transform2<float>;
} // namespace retro
