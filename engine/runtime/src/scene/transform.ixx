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
    export struct Transform
    {
        Vector2f position{};
        float rotation{0};
        Vector2f scale{1, 1};

        Matrix3x3f world_matrix = Matrix3x3f::identity();
        bool dirty = true;

        [[nodiscard]] constexpr Matrix3x3f local_matrix() const noexcept
        {
            return create_translation(position) * create_rotation(rotation) * create_scale(scale);
        }
    };

    export struct Hierarchy
    {
        entt::entity parent = entt::null;
        entt::entity first_child = entt::null;
        entt::entity next_sibling = entt::null;
    };
} // namespace retro
