/**
 * @file scene2d.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.scene2d;

import std;
import boost;
import retro.core;
import :scene.entity;
import :scene.rendering;

namespace retro
{
    export class RETRO_API Scene2D
    {
      public:
        Scene2D() = default;
        ~Scene2D() = default;

        Scene2D(const Scene2D &) = delete;
        Scene2D(Scene2D &&) = default;
        Scene2D &operator=(const Scene2D &) = delete;
        Scene2D &operator=(Scene2D &&) = default;

        RenderProxyManager &render_proxy_manager()
        {
            return render_proxy_manager_;
        }

        boost::optional<Entity &> get_entity(EntityID id);

        Entity &create_entity(const Transform &transform = {}) noexcept;

        void destroy_entity(EntityID id);

        boost::optional<Component &> get_component(ComponentID id);

        template <std::derived_from<Component> T, typename... Args>
            requires std::constructible_from<T, ComponentID, EntityID, Args...>
        T &create_component(EntityID entity_id, Args &&...args)
        {
            auto &component = components_.emplace_as<T>(entity_id, std::forward<Args>(args)...);
            component->on_attach();
            return static_cast<T &>(*component);
        }

        void destroy_component(ComponentID component_id);

      private:
        PackedPool<Entity> entities_{};
        PackedPool<Polymorphic<Component>> components_{};

        RenderProxyManager render_proxy_manager_;
    };
} // namespace retro
