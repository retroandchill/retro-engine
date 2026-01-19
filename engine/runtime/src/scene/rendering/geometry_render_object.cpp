/**
 * @file geometry_render_object.cpp
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
    struct SceneData
    {
        Vector2f viewport_size{};
        Vector2f transform_position{};
        float transform_rotation{};
        Vector2f transform_scale{};
    };

    const Name GeometryRenderPipeline::TYPE_ID = "geometry"_name;

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

    void GeometryRenderPipeline::collect_draw_calls(const entt::registry &registry, const Vector2u viewport_size)
    {
        for (const auto view = registry.view<GeometryRenderObject, Transform>();
             auto [entity, geometry, transform] : view.each())
        {
            GeometryDrawCall draw_call{.geometry = geometry.geometry,
                                       .push_constants = std::vector<std::byte>(sizeof(GeometryRenderData))};

            write_to_buffer(draw_call.push_constants,
                            GeometryRenderData{.viewport_size = Vector2f{static_cast<float>(viewport_size.x),
                                                                         static_cast<float>(viewport_size.y)},
                                               .world_matrix = transform.world_matrix,
                                               .has_texture = 0});

            pending_geometry_.push_back(std::move(draw_call));
        }
    }

    void GeometryRenderPipeline::execute(RenderContext &context)
    {
        context.draw_geometry(pending_geometry_);
    }
} // namespace retro
