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

namespace retro
{
    export using ComponentHandle = Polymorphic<RenderObject, PolymorphicType::Copyable, 128>;

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

        boost::optional<Viewport &> get_entity(ViewportID id);

        Viewport &create_viewport() noexcept;

        void destroy_viewport(ViewportID id);

        boost::optional<RenderObject &> get_render_object(RenderObjectID id);

        template <std::derived_from<RenderObject> T, typename... Args>
            requires std::constructible_from<T, RenderObjectID, ViewportID, Args...>
        T &create_render_object(ViewportID entity_id, Args &&...args)
        {
            auto &component = render_objects_.emplace_as<T>(entity_id, std::forward<Args>(args)...);
            component->on_attach();
            return static_cast<T &>(*component);
        }

        void destroy_render_object(RenderObjectID render_object_id);

      private:
        PackedPool<Viewport> viewports_{};
        PackedPool<ComponentHandle> render_objects_{};

        RenderProxyManager render_proxy_manager_;
    };
} // namespace retro
