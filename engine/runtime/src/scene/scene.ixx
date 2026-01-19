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
    export using RenderObjectHandle = Polymorphic<RenderObject, PolymorphicType::Copyable, 128>;

    export class RETRO_API Scene
    {
      public:
        Scene() = default;
        ~Scene() = default;

        Scene(const Scene &) = delete;
        Scene(Scene &&) = default;
        Scene &operator=(const Scene &) = delete;
        Scene &operator=(Scene &&) = default;

        RenderProxyManager &render_proxy_manager()
        {
            return render_proxy_manager_;
        }

        entt::entity create_entity();

        entt::entity create_viewport(Vector2f view_size);

        void destroy_entity(entt::entity entity);

        template <typename T>
        T &get_component(const entt::entity entity)
        {
            return registry_.get<T>(entity);
        }

        void update_transforms();

      private:
        void update_transform(entt::entity entity, const Matrix3x3f &parentWorld);

        entt::registry registry_{};
        RenderProxyManager render_proxy_manager_{};
    };
} // namespace retro
