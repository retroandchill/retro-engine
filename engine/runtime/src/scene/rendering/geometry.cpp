/**
 * @file geometry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import retro.logging;

namespace retro
{
    namespace
    {
        const auto RECTANGLE = std::make_shared<const Geometry>(
            std::vector{
                Vertex{Vector2f{0, 0}, Vector2f{0, 0}},
                Vertex{Vector2f{1, 0}, Vector2f{1, 0}},
                Vertex{Vector2f{1, 1}, Vector2f{1, 1}},
                Vertex{Vector2f{0, 1}, Vector2f{0, 1}},
            },
            std::vector<uint32>{0, 2, 1, 2, 0, 3});

        const auto TRIANGLE =
            std::make_shared<const Geometry>(std::vector{Vertex{Vector2f{0.5f, 0.5f}, Vector2f{0.5f, 0.5f}},
                                                         Vertex{Vector2f{1, 0}, Vector2f{1, 0}},
                                                         Vertex{Vector2f{0, 1}, Vector2f{0, 1}}},
                                             std::vector<uint32>{0, 1, 2});
    } // namespace

    void GeometryObject::set_geometry(GeometryType type)
    {
        switch (type)
        {
            case GeometryType::Rectangle:
                geometry_ = RECTANGLE;
                break;
            case GeometryType::Triangle:
                geometry_ = TRIANGLE;
                break;
            case GeometryType::None:
            case GeometryType::Custom:
            default:
                geometry_ = nullptr;
                break;
        }
    }

    std::type_index GeometryRenderPipeline::component_type() const
    {
        return typeid(GeometryObject);
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
            auto *geometry = node->geometry().get();
            if (geometry == nullptr)
                continue;

            const auto &transform = node->transform();

            InstanceData instance{.transform = transform.matrix(),
                                  .translation = transform.translation(),
                                  .pivot = node->pivot(),
                                  .size = node->size(),
                                  .color = node->color(),
                                  .has_texture = 0};

            auto &batch = geometry_batches_[geometry];
            batch.geometry = geometry;
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
