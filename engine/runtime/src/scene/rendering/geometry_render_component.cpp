/**
 * @file geometry_render_component.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import retro.logging;

namespace retro
{
    std::type_index GeometryRenderPipeline::component_type() const
    {
        return typeid(GeometryObject);
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
        geometry_batches_.clear();
    }

    void GeometryRenderPipeline::collect_draw_calls(Scene &registry, const Vector2u viewport_size)
    {
        geometry_batches_.clear();

        for (auto *node : registry.nodes_of_type<GeometryObject>())
        {
            const auto &transform = node->transform();

            InstanceData instance{.translation = transform.translation(),
                                  .transform = transform.matrix(),
                                  // TODO: Add pivot and size
                                  .has_texture = 0};

            auto &batch = geometry_batches_[&node->geometry()];
            batch.geometry = &node->geometry();
            batch.viewport_size = Vector2f{static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)};
            batch.instances.push_back(instance);
        }
    }

    void GeometryRenderPipeline::execute(RenderContext &context)
    {
        std::vector<GeometryBatch> batches;
        for (auto &batch : geometry_batches_ | std::views::values)
        {
            batches.push_back(std::move(batch));
        }
        context.draw_geometry(batches);
    }
} // namespace retro
