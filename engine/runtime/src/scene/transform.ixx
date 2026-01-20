/**
 * @file transform.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.transform;

import retro.core;
import std;
import entt;

namespace retro
{
    export class Transform
    {
      public:
        [[nodiscard]] constexpr Vector2f position() const noexcept
        {
            return position_;
        }

        constexpr void set_position(const Vector2f positon) noexcept
        {
            position_ = positon;
            dirty_ = true;
        }

        [[nodiscard]] constexpr float rotation() const noexcept
        {
            return rotation_;
        }

        constexpr void set_rotation(const float rotation) noexcept
        {
            rotation_ = rotation;
        }

        [[nodiscard]] constexpr Vector2f scale() const noexcept
        {
            return scale_;
        }

        constexpr void set_scale(const Vector2f scale) noexcept
        {
            scale_ = scale;
        }

        [[nodiscard]] constexpr Matrix3x3f local_matrix() const noexcept
        {
            return create_translation(position_) * create_rotation(rotation_) * create_scale(scale_);
        }

        [[nodiscard]] constexpr const Matrix3x3f &world_matrix() const noexcept
        {
            return world_matrix_;
        }

      private:
        friend class Scene;

        Vector2f position_{};
        float rotation_{0};
        Vector2f scale_{1, 1};

        Matrix3x3f world_matrix_ = Matrix3x3f::identity();
        bool dirty_ = true;
    };

    export struct Hierarchy
    {
        entt::entity parent = entt::null;
        entt::entity first_child = entt::null;
        entt::entity next_sibling = entt::null;
    };
} // namespace retro
