/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene;

import std;
import boost;
import retro.core;
import :scene.rendering;
import entt;

namespace retro
{
    export class Scene;

    export struct Viewport
    {
        Vector2f view_size{};
    };

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

    class RETRO_API Scene
    {
      public:
        explicit Scene(entt::registry &registry, PipelineManager &pipeline_manager);

        ~Scene() = default;

        Scene(const Scene &) = delete;
        Scene(Scene &&) = default;
        Scene &operator=(const Scene &) = delete;
        Scene &operator=(Scene &&) = default;

        entt::entity create_entity();

        entt::entity create_viewport(Vector2f view_size);

        void destroy_entity(entt::entity entity);

        void attach_to_parent(entt::entity child, entt::entity parent);

        void detach_from_parent(entt::entity entity);

        template <typename T>
        T &get_component(const entt::entity entity)
        {
            return registry_->get<T>(entity);
        }

        template <RenderComponent T, typename... Args>
            requires std::constructible_from<T, Args...>
        std::pair<entt::entity, T &> create_render_component(const entt::entity parent, Args &&...args)
        {
            auto entity = create_entity();
            registry_->emplace<T>(entity, std::forward<Args>(args)...);
            attach_to_parent(entity, parent);
            return {entity, registry_->get<T>(entity)};
        }

        void update_transforms();

        void collect_draw_calls(Vector2u viewport_size);

      private:
        void update_transform(entt::entity entity, const Matrix3x3f &parentWorld, bool parent_changed);

        entt::registry *registry_{};
        PipelineManager *pipeline_manager_;
    };
} // namespace retro
