/**
 * @file sprite.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.runtime.rendering.objects.sprite;

namespace retro
{
    DrawCommand SpriteBatch::create_draw_command() const
    {
        constexpr std::size_t indices_per_sprite = 6;
        return DrawCommand{
            .instance_buffers = {as_bytes(std::span{instances})},
            .descriptor_sets = {texture},
            .push_constants = as_bytes(std::span{&viewport_draw_info, 1}),
            .index_count = indices_per_sprite,
            .instance_count = instances.size(),
        };
    }

    std::type_index SpriteRenderPipeline::component_type() const
    {
        return typeid(Sprite);
    }

    const ShaderLayout &SpriteRenderPipeline::shaders() const
    {
        static const ShaderLayout layout{
            .vertex_shader = "shaders/sprite.vert.spv",
            .fragment_shader = "shaders/sprite.frag.spv",
            .vertex_bindings = {VertexInputBinding{
                .type = VertexInputType::instance,
                .stride = sizeof(SpriteInstanceData),
                .attributes =
                    {
                        // Vulkan doesn't have matrix attributes, so the most compatible option is to
                        // just treat a matrix as an array of vectors
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, transform)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, transform) + sizeof(Vector2f)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, translation)},
                        VertexAttribute{.type = ShaderDataType::int32,
                                        .size = sizeof(std::int32_t),
                                        .offset = offsetof(SpriteInstanceData, z_order)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, pivot)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, size)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, min_uv)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, max_uv)},
                        VertexAttribute{.type = ShaderDataType::vec4,
                                        .size = sizeof(Color),
                                        .offset = offsetof(SpriteInstanceData, tint)},
                    },
            }},
            .descriptor_bindings =
                {
                    DescriptorBinding{.type = DescriptorType::combined_image_sampler,
                                      .stages = ShaderStage::fragment,
                                      .count = 1},
                },
            .push_constant_bindings =
                PushConstantBinding{.stages = ShaderStage::vertex, .size = sizeof(ViewportDrawInfo), .offset = 0}};
        return layout;
    }

    SmallUniquePtr<DrawCommandSource> SpriteRenderPipeline::collect_draw_calls_source(
        const SceneNodeList &nodes,
        const Vector2u viewport_size,
        const Viewport &viewport,
        std::pmr::memory_resource &memory_resource)
    {
        std::pmr::unordered_map<const Texture *, SpriteBatch> batches{&memory_resource};
        for (const auto *node : nodes.nodes_of_type<Sprite>())
        {
            auto *texture = node->texture().get();
            if (texture == nullptr)
                continue;

            const auto &transform = node->transform();

            SpriteInstanceData instance{.transform = transform.matrix(),
                                        .translation = transform.translation(),
                                        .z_order = node->z_order(),
                                        .pivot = node->pivot(),
                                        .size = node->size(),
                                        .min_uv = node->uvs().min,
                                        .max_uv = node->uvs().max,
                                        .tint = node->tint()};

            if (auto it = batches.find(texture); it == batches.end())
            {
                auto [pair, inserted] =
                    batches.emplace(texture,
                                    SpriteBatch{
                                        .texture = texture,
                                        .instances = std::pmr::vector<SpriteInstanceData>{&memory_resource},
                                        .viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size),
                                    });

                pair->second.instances.push_back(instance);
            }
            else
            {
                auto &[draw_texture, instances, viewport_draw_info] = batches[texture];
                draw_texture = texture;
                viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size);
                instances.push_back(instance);
            }
        }

        return DrawCommandSource::from(std::move(batches) | std::views::values |
                                       std::ranges::to<std::pmr::vector<SpriteBatch>>(&memory_resource));
    }
} // namespace retro
