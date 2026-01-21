/**
 * @file geometry_render_component.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.geometry_render_component;

import std;
import retro.core;
import :scene.rendering;

namespace retro
{
    export class GeometryRenderPipeline;

    export struct GeometryRenderComponent
    {
        using PipelineType = GeometryRenderPipeline;

        Geometry geometry{};
    };

    export struct GeometryRenderData
    {
        Vector2f viewport_size{};
        std::array<float, 2> _padding{}; // Align columns to 16 bytes
        std::array<Vector4f, 3> world_matrix{};
        uint32 has_texture{};
    };

    class RETRO_API GeometryRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] usize push_constants_size() const override;

        [[nodiscard]] PipelineShaders shaders() const override;

        void clear_draw_queue() override;

        void collect_draw_calls(const entt::registry &registry, Vector2u viewport_size, SingleArena &arena) override;

        void execute(RenderContext &context) override;

      private:
        friend struct GeometryType;

        std::vector<GeometryDrawCall> pending_geometry_;
    };
} // namespace retro
