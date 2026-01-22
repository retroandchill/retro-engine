/**
 * @file geometry_render_component.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

// Workaround for IntelliSense issues regarding entt
#ifdef __JETBRAINS_IDE__
#include <entt/entt.hpp>
#endif

module retro.runtime;

import retro.logging;

namespace retro
{
    std::type_index GeometryRenderPipeline::component_type() const
    {
        return typeid(GeometryRenderComponent);
    }

    usize GeometryRenderPipeline::push_constants_size() const
    {
        return sizeof(GeometryRenderData);
    }

    PipelineShaders GeometryRenderPipeline::shaders() const
    {

        return {"shaders/geometry.vert.spv", "shaders/geometry.frag.spv"};
    }

    void GeometryRenderPipeline::clear_draw_queue()
    {
        pending_geometry_.clear();
    }

    void GeometryRenderPipeline::collect_draw_calls(const Vector2u viewport_size, SingleArena &arena)
    {
        for (const auto view = registry_->view<GeometryRenderComponent, Transform>();
             auto [entity, geometry, transform] : view.each())
        {
            GeometryDrawCall draw_call{arena, geometry.geometry, sizeof(GeometryRenderData)};

            const auto &matrix = transform.world_matrix();
            write_to_buffer(
                draw_call.push_constants,
                GeometryRenderData{
                    .viewport_size = Vector2f{static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)},
                    .world_matrix = {Vector4f(matrix[0, 0], matrix[1, 0], matrix[2, 0], 0.0f),
                                     Vector4f(matrix[0, 1], matrix[1, 1], matrix[2, 1], 0.0f),
                                     Vector4f(matrix[0, 2], matrix[1, 2], matrix[2, 2], 0.0f)},
                    .has_texture = 0});

            pending_geometry_.push_back(std::move(draw_call));
        }
    }

    void GeometryRenderPipeline::execute(RenderContext &context)
    {
        context.draw_geometry(pending_geometry_);
    }
} // namespace retro
