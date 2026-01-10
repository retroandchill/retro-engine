/**
 * @file render_object.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.render_object;

import std;
import retro.core;
import :scene.actor_ptr;
import :scene.rendering;
import :scene.transform;

namespace retro
{
    export class RETRO_API RenderObject
    {
      public:
        using IdType = RenderObjectID;

      protected:
        inline explicit RenderObject(const RenderObjectID id,
                                     const ViewportID viewport_id,
                                     const Transform &transform = {})
            : id_{id}, viewport_{viewport_id}, transform_{transform}
        {
        }

      public:
        virtual ~RenderObject() = default;

        [[nodiscard]] inline RenderObjectID id() const
        {
            return id_;
        }

        [[nodiscard]] Viewport &viewport() const noexcept;

        [[nodiscard]] inline const Transform &transform() const noexcept
        {
            return transform_;
        }

        inline void set_transform(const Transform &transform) noexcept
        {
            transform_ = transform;
        }

        [[nodiscard]] inline Vector2f position() const noexcept
        {
            return transform_.position;
        }

        inline void set_position(const Vector2f position) noexcept
        {
            transform_.position = position;
        }

        [[nodiscard]] inline float rotation() const noexcept
        {
            return transform_.rotation;
        }

        inline void set_rotation(const float rotation) noexcept
        {
            transform_.rotation = rotation;
        }

        [[nodiscard]] inline Vector2f scale() const noexcept
        {
            return transform_.scale;
        }

        inline void set_scale(const Vector2f scale) noexcept
        {
            transform_.scale = scale;
        }

        void on_attach();
        void on_detach();

      protected:
        virtual RenderProxyID create_render_proxy(RenderProxyManager &proxy_manager) = 0;

        virtual void destroy_render_proxy(RenderProxyManager &proxy_manager, RenderProxyID id) = 0;

      private:
        RenderObjectID id_;
        ActorPtr<Viewport> viewport_;
        Transform transform_;
        std::optional<RenderProxyID> render_proxy_id_;
    };

} // namespace retro
