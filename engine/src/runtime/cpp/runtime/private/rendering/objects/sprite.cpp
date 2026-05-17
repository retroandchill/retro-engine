/**
 * @file sprite.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.runtime.rendering.objects.sprite;

import retro.runtime.rendering.shaders;

namespace retro
{
    DrawCommand SpriteBatch::create_draw_command() const
    {
        constexpr std::size_t indices_per_sprite = 6;
        return DrawCommand{
            .instance_buffers = {as_bytes(std::span{instances})},
            .descriptor_sets = {texture.get()},
            .push_constants = as_bytes(std::span{&viewport_draw_info, 1}),
            .index_count = indices_per_sprite,
            .instance_count = instances.size(),
        };
    }

    void Sprite::set_texture(RefCountPtr<Texture> texture) noexcept
    {
        texture_ = std::move(texture);
        mark_cached_render_data_as_dirty();
    }

    void Sprite::set_pivot(const Vector2f pivot) noexcept
    {
        pivot_ = pivot;
        mark_cached_render_data_as_dirty();
    }

    void Sprite::set_size(const Vector2f size) noexcept
    {
        size_ = size;
        mark_cached_render_data_as_dirty();
    }

    void Sprite::set_uvs(const UVs &uvs) noexcept
    {
        uvs_ = uvs;
        mark_cached_render_data_as_dirty();
    }

    void Sprite::set_draw_mode(const SpriteDrawMode draw_mode) noexcept
    {
        draw_mode_ = draw_mode;
        mark_cached_render_data_as_dirty();
    }

    void Sprite::set_margin(const Margin &margin) noexcept
    {
        margin_ = margin;
        mark_cached_render_data_as_dirty();
    }

    void Sprite::on_world_transform_updated()
    {
        SceneNode::on_world_transform_updated();
        mark_cached_render_data_as_dirty();
    }

    void Sprite::mark_cached_render_data_as_dirty()
    {
        render_data_dirty_ = true;
    }

    void Sprite::refresh_cached_render_data()
    {
        if (!render_data_dirty_)
            return;

        cached_quads_.clear();
        switch (draw_mode_)
        {
            case SpriteDrawMode::quad:
                cached_quads_.push_back({.transform = world_transform(), .uvs = uvs_, .pivot = pivot_, .size = size_});
                break;
            case SpriteDrawMode::box:
                {
                    cached_quads_.reserve(9);
                    auto left = std::min(margin_.left, size_.x);
                    auto right = std::min(margin_.right, size_.x);
                    auto top = std::min(margin_.top, size_.y);
                    auto bottom = std::min(margin_.bottom, size_.y);

                    if (left + right > size_.x)
                    {
                        right = size_.x - left;
                    }

                    if (top + bottom > size_.y)
                    {
                        bottom = size_.y - top;
                    }

                    const auto destination_x = std::array{
                        0.0f,
                        left,
                        size_.x - right,
                        size_.x,
                    };

                    const auto destination_y = std::array{
                        0.0f,
                        top,
                        size_.y - bottom,
                        size_.y,
                    };

                    const auto uv_x = std::array{
                        uvs_.min.x,
                        uvs_.min.x + left / static_cast<float>(texture_->width()),
                        uvs_.max.x - right / static_cast<float>(texture_->width()),
                        uvs_.max.x,
                    };

                    const auto uv_y = std::array{
                        uvs_.min.y,
                        uvs_.min.y + top / static_cast<float>(texture_->height()),
                        uvs_.max.y - bottom / static_cast<float>(texture_->height()),
                        uvs_.max.y,
                    };

                    const auto pivot_offset = pivot_ * size_;

                    for (std::size_t y = 0; y < 3; y++)
                    {
                        for (std::size_t x = 0; x < 3; x++)
                        {
                            const auto min_position = Vector2f{
                                destination_x[x],
                                destination_y[y],
                            };

                            const auto max_position = Vector2f{
                                destination_x[x + 1],
                                destination_y[y + 1],
                            };

                            if (const auto quad_size = max_position - min_position; quad_size == Vector2f::zero())
                                continue;

                            const auto local_min_position = min_position - pivot_offset;
                            const auto world_min_position =
                                world_transform().matrix() * local_min_position + world_transform().translation();
                            const auto local_max_position = max_position - pivot_offset;
                            const auto world_max_position =
                                world_transform().matrix() * local_max_position + world_transform().translation();

                            const auto min_uv = Vector2f{
                                uv_x[x],
                                uv_y[y],
                            };

                            const auto max_uv = Vector2f{
                                uv_x[x + 1],
                                uv_y[y + 1],
                            };

                            cached_quads_.push_back({
                                Transform2f{world_min_position},
                                UVs{min_uv, max_uv},
                                {0, 0},
                                world_max_position - world_min_position,
                            });
                        }
                    }

                    break;
                }
        }

        render_data_dirty_ = false;
    }

    std::type_index SpriteRenderPipeline::component_type() const
    {
        return typeid(Sprite);
    }

    const ShaderLayout &SpriteRenderPipeline::shaders() const
    {
        static const ShaderLayout layout{
            .vertex_shader = shaders::sprite_vert,
            .fragment_shader = shaders::sprite_frag,
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
        for (auto *node : nodes.nodes_of_type<Sprite>())
        {
            auto &texture = node->texture();
            if (texture == nullptr)
                continue;

            node->refresh_cached_render_data();

            auto pending_data = node->cached_quads_ | std::views::transform(
                                                          [node](const SpriteQuad &quad)
                                                          {
                                                              return SpriteInstanceData{
                                                                  .transform = quad.transform.matrix(),
                                                                  .translation = quad.transform.translation(),
                                                                  .z_order = node->z_order(),
                                                                  .pivot = quad.pivot,
                                                                  .size = quad.size,
                                                                  .min_uv = quad.uvs.min,
                                                                  .max_uv = quad.uvs.max,
                                                                  .tint = node->tint_,
                                                              };
                                                          });

            if (auto it = batches.find(texture.get()); it == batches.end())
            {
                auto [pair, inserted] =
                    batches.emplace(texture.get(),
                                    SpriteBatch{
                                        .texture = texture,
                                        .instances = std::pmr::vector<SpriteInstanceData>{&memory_resource},
                                        .viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size),
                                    });

                pair->second.instances.append_range(pending_data);
            }
            else
            {
                auto &[draw_texture, instances, viewport_draw_info] = batches[texture.get()];
                draw_texture = texture;
                viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size);
                instances.append_range(pending_data);
            }
        }

        return DrawCommandSource::from(std::move(batches) | std::views::values |
                                       std::ranges::to<std::pmr::vector<SpriteBatch>>(&memory_resource));
    }
} // namespace retro
