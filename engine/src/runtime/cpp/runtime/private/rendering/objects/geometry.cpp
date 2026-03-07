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
            .push_constants = as_bytes(std::span{&viewport_draw_info, 1}),
            .index_count = geometry->indices.size(),
            .instance_count = instances.size(),
        };
    }

    void GeometryObject::set_geometry(GeometryType type)
    {
        switch (type)
        {
            case GeometryType::rectangle:
                geometry_ = RECTANGLE;
                break;
            case GeometryType::triangle:
                geometry_ = TRIANGLE;
                break;
            case GeometryType::none:
            case GeometryType::custom:
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
            .vertex_bindings = {VertexInputBinding{.type = VertexInputType::vertex,
                                                   .stride = sizeof(Vertex),
                                                   .attributes = {VertexAttribute{.type = ShaderDataType::vec2,
                                                                                  .size = sizeof(Vector2f),
                                                                                  .offset = offsetof(Vertex, position)},
                                                                  VertexAttribute{.type = ShaderDataType::vec2,
                                                                                  .size = sizeof(Vector2f),
                                                                                  .offset = offsetof(Vertex, uv)}}},
                                VertexInputBinding{
                                    .type = VertexInputType::instance,
                                    .stride = sizeof(GeometryInstanceData),
                                    .attributes =
                                        {// Vulkan doesn't have matrix attributes, so the most compatible option is to
                                         // just treat a matrix as an array of vectors
                                         VertexAttribute{.type = ShaderDataType::vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, transform)},
                                         VertexAttribute{.type = ShaderDataType::vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, transform) +
                                                                   sizeof(Vector2f)},
                                         VertexAttribute{.type = ShaderDataType::vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, translation)},
                                         VertexAttribute{.type = ShaderDataType::int32,
                                                         .size = sizeof(std::int32_t),
                                                         .offset = offsetof(GeometryInstanceData, z_order)},
                                         VertexAttribute{.type = ShaderDataType::vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, pivot)},
                                         VertexAttribute{.type = ShaderDataType::vec2,
                                                         .size = sizeof(Vector2f),
                                                         .offset = offsetof(GeometryInstanceData, size)},
                                         VertexAttribute{.type = ShaderDataType::vec4,
                                                         .size = sizeof(Color),
                                                         .offset = offsetof(GeometryInstanceData, color)},
                                         VertexAttribute{.type = ShaderDataType::uint32,
                                                         .size = sizeof(std::uint32_t),
                                                         .offset = offsetof(GeometryInstanceData, has_texture)}}}},
            .push_constant_bindings =
                PushConstantBinding{.stages = ShaderStage::vertex, .size = sizeof(ViewportDrawInfo), .offset = 0}};

        return layout;
    }

    SmallUniquePtr<DrawCommandSource> GeometryRenderPipeline::collect_draw_calls_source(
        const SceneNodeList &nodes,
        Vector2u viewport_size,
        const Viewport &viewport,
        std::pmr::memory_resource &memory_resource)
    {
        std::pmr::unordered_map<const Geometry *, GeometryBatch> geometry_batches{&memory_resource};
        for (const auto *node : nodes.nodes_of_type<GeometryObject>())
        {
            auto *geometry = node->geometry().get();
            if (geometry == nullptr)
                continue;

            const auto &transform = node->transform();

            GeometryInstanceData instance{.transform = transform.matrix(),
                                          .translation = transform.translation(),
                                          .z_order = node->z_order(),
                                          .pivot = node->pivot(),
                                          .size = node->size(),
                                          .color = node->color(),
                                          .has_texture = 0};

            if (auto it = geometry_batches.find(geometry); it == geometry_batches.end())
            {
                auto [pair, inserted] = geometry_batches.emplace(
                    geometry,
                    GeometryBatch{
                        .geometry = geometry,
                        .instances = std::pmr::vector<GeometryInstanceData>{&memory_resource},
                        .viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size),
                    });

                pair->second.instances.push_back(instance);
            }
            else
            {
                auto &batch = it->second;
                batch.geometry = geometry;
                batch.viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size);
                batch.instances.push_back(instance);
            }
        }

        return DrawCommandSource::from(std::move(geometry_batches) | std::views::values |
                                       std::ranges::to<std::pmr::vector<GeometryBatch>>(&memory_resource));
    }
} // namespace retro
