/**
 * @file matrix.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:math.matrix;

import :defines;
import std;
import :math.concepts;
import :math.vector;

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
