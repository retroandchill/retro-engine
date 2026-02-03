/**
 * @file geometry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.runtime.rendering.objects.geometry;

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
            std::vector<std::uint32_t>{0, 2, 1, 2, 0, 3});

        const auto TRIANGLE =
            std::make_shared<const Geometry>(std::vector{Vertex{Vector2f{0.5f, 0.5f}, Vector2f{0.5f, 0.5f}},
                                                         Vertex{Vector2f{1, 0}, Vector2f{1, 0}},
                                                         Vertex{Vector2f{0, 1}, Vector2f{0, 1}}},
                                             std::vector<std::uint32_t>{0, 1, 2});
    } // namespace

    DrawCommand GeometryBatch::create_draw_command() const
    {
        return DrawCommand{
            .vertex_buffers =
                {
                    as_bytes(std::span{geometry->vertices}),
                },
            .instance_buffers = {as_bytes(std::span{instances})},
            .index_buffer = as_bytes(std::span{geometry->indices}),
            .push_constants = as_bytes(std::span{&viewport_size, 1}),
            .index_count = geometry->indices.size(),
            .instance_count = instances.size(),
        };
    }

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

    const ShaderLayout &GeometryRenderPipeline::shaders() const
    {
        static const ShaderLayout layout{
            .vertex_shader = "shaders/geometry.vert.spv",
            .fragment_shader = "shaders/geometry.frag.spv",
            .vertex_bindings = {VertexInputBinding{.type = VertexInputType::Vertex,
                                                   .stride = sizeof(Vertex),
                                                   .attributes = {VertexAttribute{.type = ShaderDataType::Vec2,
                                                                                  .size = sizeof(Vector2f),
                                                                                  .offset = offsetof(Vertex, position)},
                                                                  VertexAttribute{.type = ShaderDataType::Vec2,
                                                                                  .size = sizeof(Vector2f),
                                                                                  .offset = offsetof(Vertex, uv)}}},
                                VertexInputBinding{
                                    .type = VertexInputType::Instance,
                                    .stride = sizeof(GeometryInstanceData),
                                    .attributes =
                                        {// Vulkan doesn't have matrix attributes, so the most compatible option is to
                                         // just treat a matrix as an array of vectors
                                         VertexAttribute{.type = ShaderDataType::Vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, transform)},
                                         VertexAttribute{.type = ShaderDataType::Vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, transform) +
                                                                   sizeof(Vector2f)},
                                         VertexAttribute{.type = ShaderDataType::Vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, translation)},
                                         VertexAttribute{.type = ShaderDataType::Vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, pivot)},
                                         VertexAttribute{.type = ShaderDataType::Vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, size)},
                                         VertexAttribute{.type = ShaderDataType::Vec4,
                                                         .size = sizeof(Color),
                                                         .offset = offsetof(GeometryInstanceData, color)},
                                         VertexAttribute{.type = ShaderDataType::Uint32,
                                                         .size = sizeof(std::uint32_t),
                                                         .offset = offsetof(GeometryInstanceData, has_texture)}}}},
            .push_constant_bindings =
                PushConstantBinding{.stages = ShaderStage::Vertex, .size = sizeof(Vector2f), .offset = 0}};

        return layout;
    }

    void GeometryRenderPipeline::clear_draw_queue()
    {
        geometry_batches_.clear();
    }

    void GeometryRenderPipeline::collect_draw_calls(Scene &registry, const Vector2u viewport_size)
    {
        for (const auto *node : registry.nodes_of_type<GeometryObject>())
        {
            auto *geometry = node->geometry().get();
            if (geometry == nullptr)
                continue;

            const auto &transform = node->transform();

            GeometryInstanceData instance{.transform = transform.matrix(),
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
        std::vector<DrawCommand> draw_calls;
        batches.reserve(geometry_batches_.size());
        draw_calls.reserve(geometry_batches_.size());
        for (auto &batch : geometry_batches_ | std::views::values)
        {
            auto &moved_batch = batches.emplace_back(std::move(batch));
            draw_calls.emplace_back(moved_batch.create_draw_command());
        }

        context.draw(draw_calls, shaders());
    }
} // namespace retro
