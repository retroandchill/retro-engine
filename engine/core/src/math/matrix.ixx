/**
 * @file matrix.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.math.matrix;

import std;
import retro.core.type_traits.basic;
import retro.core.math.vector;
import retro.core.math.operations;

namespace retro
{
    export template <Numeric T, std::size_t Rows, std::size_t Columns>
    class Matrix;

    export template <Numeric T, std::size_t N>
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

    export template <Numeric T, std::size_t N>
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
            T shear_x = angles.x == 0 ? 0 : 1.0f / tan(degrees_to_radians(90 - clamp(angles.x, -89.f, 89.f)));
            T shear_y = angles.x == 0 ? 0 : 1.0f / tan(degrees_to_radians(90 - clamp(angles.u, -89.f, 89.f)));
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

    export template <Numeric T, std::size_t N>
    class Quaternion;

    export template <Numeric T>
    class Quaternion<T, 2>
    {
      public:
        constexpr Quaternion() = default;

        explicit constexpr Quaternion(T radians) : rotation_{cos(radians), sin(radians)}
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

    template <Numeric T, std::size_t Rows, std::size_t Columns>
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
            for (std::size_t i = 0; i < Rows; ++i)
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

        [[nodiscard]] constexpr T &operator[](const std::size_t row, const std::size_t columns) noexcept
        {
            return data_[row + columns * Rows];
        }

        [[nodiscard]] constexpr const T &operator[](const std::size_t row, const std::size_t columns) const noexcept
        {
            return data_[row + columns * Rows];
        }

        [[nodiscard]] constexpr Matrix operator+(const Matrix &rhs) const noexcept
        {
            Matrix result;
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] + rhs.data_[i];
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator+=(const Matrix &rhs) const noexcept
        {
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                data_[i] += rhs.data_[i];
            return *this;
        }

        [[nodiscard]] constexpr Matrix operator-(const Matrix &rhs) const noexcept
        {
            Matrix result;
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] - rhs.data_[i];
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator-=(const Matrix &rhs) const noexcept
        {

            for (std::size_t i = 0; i < Rows * Columns; ++i)
                data_[i] -= rhs.data_[i];
            return *this;
        }

        template <std::size_t OtherCols>
        [[nodiscard]] constexpr Matrix<T, Rows, OtherCols> operator*(
            const Matrix<T, Columns, OtherCols> &rhs) const noexcept
        {
            Matrix<T, Rows, OtherCols> result;
            for (std::size_t j = 0; j < OtherCols; ++j)
            {
                for (std::size_t k = 0; k < Columns; ++k)
                {
                    for (std::size_t i = 0; i < Rows; ++i)
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
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] * rhs;
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator*=(const T &rhs) const noexcept
        {
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                data_[i] *= rhs;
            return *this;
        }

        [[nodiscard]] constexpr Matrix &operator*=(const Matrix &rhs) noexcept
            requires(Rows == Columns)
        {
            for (std::size_t j = 0; j < Columns; ++j)
            {
                for (std::size_t k = 0; k < Columns; ++k)
                {
                    for (std::size_t i = 0; i < Rows; ++i)
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
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                result.data_[i] = data_[i] / rhs;
            return result;
        }

        [[nodiscard]] constexpr Matrix &operator/=(const T &rhs) const noexcept
        {
            for (std::size_t i = 0; i < Rows * Columns; ++i)
                data_[i] /= rhs;
            return *this;
        }

        static constexpr Matrix identity() noexcept
            requires(Rows == Columns)
        {
            Matrix m;
            for (std::size_t i = 0; i < Rows; ++i)
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
        template <std::size_t... I>
        void fill_diagonal_from_vector(const Vector<T, Rows> &diagonal, std::index_sequence<I...>)
            requires(Rows == Columns)
        {
            ((data_[I * Rows + I] = get<I>(diagonal)), ...);
        }

        template <std::size_t... I>
        void fill_diagonal_from_vector_and_ones(const Vector<T, Rows> &diagonal, std::index_sequence<I...>)
            requires(Rows == Columns)
        {
            (
                [&]<std::size_t Idx>
                {
                    constexpr std::size_t row = Idx % Rows;
                    constexpr std::size_t col = Idx / Columns;

                    if constexpr (row == col)
                        data_[Idx] = get<row>(diagonal);
                    else
                        data_[Idx] = T(1);
                }.template operator()<I>(),
                ...);
        }

        std::array<T, Rows * Columns> data_{};
    };

    template <Numeric T, std::size_t N, std::size_t... I>
    Matrix<T, 1, N> to_row_matrix(const Vector<T, N> &vec, std::index_sequence<I...>)
    {
        return Matrix<T, 1, N>{get<I>(vec)...};
    }

    export template <Numeric T, std::size_t N>
    Matrix<T, 1, N> to_row_matrix(const Vector<T, N> &vec)
    {
        return to_row_matrix(vec, std::make_index_sequence<N>{});
    }

    template <Numeric T, std::size_t N, std::size_t... I>
    Vector<T, N> from_row_matrix(Matrix<T, 1, N> matrix, std::index_sequence<I...>)
    {
        return Vector<T, N>{matrix[0, I]...};
    }

    export template <Numeric T, std::size_t N>
    Vector<T, N> from_row_matrix(Matrix<T, 1, N> matrix)
    {
        return from_row_matrix(matrix, std::make_index_sequence<N>{});
    }

    template <Numeric T, std::size_t N, std::size_t... I>
    Matrix<T, N, 1> to_column_matrix(const Vector<T, N> &vec, std::index_sequence<I...>)
    {
        return Matrix<T, N, 1>{get<I>(vec)...};
    }

    export template <Numeric T, std::size_t N>
    Matrix<T, N, 1> to_column_matrix(const Vector<T, N> &vec)
    {
        return to_column_matrix(vec, std::make_index_sequence<N>{});
    }

    template <Numeric T, std::size_t N, std::size_t... I>
    Vector<T, N> from_column_matrix(Matrix<T, N, 1> matrix, std::index_sequence<I...>)
    {
        return Vector<T, N>{matrix[I, 0]...};
    }

    export template <Numeric T, std::size_t N>
    Vector<T, N> from_column_matrix(Matrix<T, N, 1> matrix)
    {
        return from_column_matrix(matrix, std::make_index_sequence<N>{});
    }

    template <typename T, std::size_t N, std::size_t M>
    constexpr bool operator==(const Matrix<T, N, M> &lhs, const Matrix<T, N, M> &rhs)
    {
        if constexpr (std::is_floating_point_v<T>)
        {
            for (std::size_t i = 0; i < N * M; ++i)
            {
                if (!nearly_equal(lhs.data_[i], rhs.data_[i], KINDA_SMALL_NUMBER))
                    return false;
            }
        }
        else
        {
            for (std::size_t i = 0; i < N * M; ++i)
            {
                if (lhs.data_[i] != rhs.data_[i])
                    return false;
            }
        }

        return true;
    }

    export template <Numeric T>
    using Matrix2x2 = Matrix<T, 2, 2>;
    export using Matrix2x2f = Matrix2x2<float>;

    export template <Numeric T>
    using Matrix3x3 = Matrix<T, 3, 3>;
    export using Matrix3x3f = Matrix3x3<float>;

    export template <Numeric T>
    using Matrix4x4 = Matrix<T, 4, 4>;
    export using Matrix4x4f = Matrix4x4<float>;
} // namespace retro
