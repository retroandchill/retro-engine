/**
 * @file sprite.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.runtime;

namespace retro
{
    DrawCommand SpriteBatch::create_draw_command() const
    {
        constexpr std::size_t INDICES_PER_SPRITE = 6;
        return DrawCommand{
            .instance_buffers = {as_bytes(std::span{instances})},
            .descriptor_sets = {texture->render_data()},
            .push_constants = as_bytes(std::span{&viewport_size, 1}),
            .index_count = INDICES_PER_SPRITE,
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
                .type = VertexInputType::Instance,
                .stride = sizeof(SpriteInstanceData),
                .attributes =
                    {
                        // Vulkan doesn't have matrix attributes, so the most compatible option is to
                        // just treat a matrix as an array of vectors
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, transform)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, transform) + sizeof(Vector2f)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, translation)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, pivot)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, size)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, min_uv)},
                        VertexAttribute{.type = ShaderDataType::Vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(SpriteInstanceData, max_uv)},
                        VertexAttribute{.type = ShaderDataType::Vec4,
                                        .size = sizeof(Color),
                                        .offset = offsetof(SpriteInstanceData, tint)},
                    },
            }},
            .descriptor_bindings =
                {
                    DescriptorBinding{.type = DescriptorType::CombinedImageSampler,
                                      .stages = ShaderStage::Fragment,
                                      .count = 1},
                },
            .push_constant_bindings =
                PushConstantBinding{.stages = ShaderStage::Vertex, .size = sizeof(Vector2f), .offset = 0}};
        return layout;
    }

    void SpriteRenderPipeline::clear_draw_queue()
    {
        batches_.clear();
    }

    void SpriteRenderPipeline::collect_draw_calls(Scene &registry, const Vector2u viewport_size)
    {
        for (const auto *node : registry.nodes_of_type<Sprite>())
        {
            auto *texture = node->texture().get();
            if (texture == nullptr)
                continue;

            const auto &transform = node->transform();

            SpriteInstanceData instance{.transform = transform.matrix(),
                                        .translation = transform.translation(),
                                        .pivot = node->pivot(),
                                        .size = node->size(),
                                        .tint = node->tint()};

            auto &batch = batches_[texture];
            batch.texture = texture;
            batch.viewport_size = Vector2f{static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)};
            batch.instances.push_back(instance);
        }
    }

    void SpriteRenderPipeline::execute(RenderContext &context)
    {
        std::vector<SpriteBatch> batches;
        std::vector<DrawCommand> draw_calls;
        batches.reserve(batches_.size());
        draw_calls.reserve(batches_.size());
        for (auto &batch : batches_ | std::views::values)
        {
            auto &moved_batch = batches.emplace_back(std::move(batch));
            draw_calls.emplace_back(moved_batch.create_draw_command());
        }

        context.draw(draw_calls, shaders());
    }
} // namespace retro
