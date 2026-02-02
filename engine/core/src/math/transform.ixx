/**
 * @file transform.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.math.transform;

import std;
import retro.core.math.matrix;
import retro.core.math.vector;

namespace retro
{
    export template <std::floating_point T, std::size_t N>
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
