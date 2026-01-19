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
import :scene.rendering.render_pipeline;
import :scene.transform;

namespace retro
{
    export struct GeometryType;

    export struct GeometryRenderObject
    {
        Geometry geometry{};
    };

    export struct GeometryRenderData
    {
        Vector2f viewport_size{};
        Matrix3x3f world_matrix{};
        uint32 has_texture{};
    };

    export class RETRO_API GeometryRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] inline Name type() const override
        {
            return TYPE_ID;
        }

        [[nodiscard]] usize push_constants_size() const override;

        [[nodiscard]] PipelineShaders shaders() const override;

        void clear_draw_queue() override;

        void collect_draw_calls(const entt::registry &registry, Vector2u viewport_size) override;

        void execute(RenderContext &context) override;

      private:
        static const Name TYPE_ID;
        friend struct GeometryType;

        std::vector<GeometryDrawCall> pending_geometry_;
    };

    struct GeometryType
    {
        using Component = GeometryRenderObject;
        using Pipeline = GeometryRenderPipeline;
        inline static Name name()
        {
            return GeometryRenderPipeline::TYPE_ID;
        }
    };
} // namespace retro
