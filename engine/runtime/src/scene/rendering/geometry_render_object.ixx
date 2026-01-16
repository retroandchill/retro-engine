/**
 * @file geometry_render_object.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.geometry_render_object;

import std;
import retro.core;
import :scene.rendering.render_object;
import :scene.rendering.render_pipeline;
import :scene.transform;

namespace retro
{
    export class RETRO_API GeometryRenderObject final : public RenderObject
    {
      public:
        inline GeometryRenderObject(const RenderObjectID id,
                                    const ViewportID viewport_id,
                                    const Transform &transform = {})
            : RenderObject{id, viewport_id, transform}
        {
        }

        RenderProxyID create_render_proxy(RenderProxyManager &proxy_manager) override;

        void destroy_render_proxy(RenderProxyManager &proxy_manager, RenderProxyID id) override;

        [[nodiscard]] inline const Geometry &geometry() const noexcept
        {
            return geometry_;
        }

        inline void set_geometry(Geometry geometry) noexcept
        {
            geometry_ = std::move(geometry);
        }

      private:
        Geometry geometry_{};
    };

    export class RETRO_API GeometryRenderProxy
    {
      public:
        using IdType = RenderProxyID;
        using DrawCallData = GeometryDrawCall;

        inline GeometryRenderProxy(const RenderProxyID id, GeometryRenderObject &object) : id_{id}, object_(&object)
        {
        }

        inline static Name type_id()
        {
            return TYPE_ID;
        }

        [[nodiscard]] inline RenderProxyID id() const
        {
            return id_;
        }

        [[nodiscard]] GeometryDrawCall get_draw_call(Vector2u viewport_size) const;

      private:
        static const Name TYPE_ID;

        RenderProxyID id_{};
        ActorPtr<GeometryRenderObject> object_{};
    };

    export struct GeometryRenderData
    {
        Vector2f viewport_size{};
        Vector2f position{};
        uint32 z_order{};
        float rotation{};
        Vector2f scale{};
    };

    export class RETRO_API GeometryRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] inline Name type() const override
        {
            return GeometryRenderProxy::type_id();
        }

        [[nodiscard]] usize push_constants_size() const override;

        [[nodiscard]] PipelineShaders shaders() const override;

        void clear_draw_queue() override;

        void queue_draw_calls(const std::any &render_data) override;

        void execute(RenderContext &context) override;

      private:
        std::vector<GeometryDrawCall> pending_geometry_;
    };
} // namespace retro
