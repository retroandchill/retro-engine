/**
 * @file scene.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.scene;

import std;
import boost;
import retro.core;
import :scene.viewport;
import :scene.rendering;
import :interfaces;
import entt;

namespace retro
{
    export class RETRO_API Scene
    {
      public:
        explicit Scene(Renderer2D *renderer);

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
            return registry_.get<T>(entity);
        }

        template <RenderComponent T, typename... Args>
            requires std::constructible_from<T, Args...>
        std::pair<entt::entity, T &> create_render_component(const entt::entity parent, Args &&...args)
        {
            auto entity = create_entity();
            registry_.emplace<T>(entity, std::forward<Args>(args)...);
            attach_to_parent(entity, parent);
            return {entity, registry_.get<T>(entity)};
        }

        void update_transforms();

        void collect_draw_calls(Vector2u viewport_size);

      private:
        void update_transform(entt::entity entity, const Matrix3x3f &parentWorld, bool parent_changed);

        entt::registry registry_{};
        PipelineManager pipeline_manager_;
    };
} // namespace retro
