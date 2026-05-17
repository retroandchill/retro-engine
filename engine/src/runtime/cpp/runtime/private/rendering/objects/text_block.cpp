/**
 * @file text_block.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.runtime.rendering.objects.text_block;

import retro.runtime.rendering.shaders;
import retro.runtime.rendering.text.font;
import retro.core.strings.encoding;

namespace retro
{
    namespace
    {
        struct PendingGlyphQuad
        {
            Vector2f position{};
            Vector2f size{};
            UVs uvs{};
        };
    } // namespace

    DrawCommand TextBlockBatch::create_draw_command() const
    {
        constexpr std::size_t indices_per_sprite = 6;
        return DrawCommand{
            .instance_buffers = {as_bytes(std::span{instances})},
            .descriptor_sets = {&font_atlas->texture()},
            .push_constants = as_bytes(std::span{&viewport_draw_info, 1}),
            .index_count = indices_per_sprite,
            .instance_count = instances.size(),
        };
    }

    void TextBlock::set_text(std::string text) noexcept
    {
        text_ = std::move(text);
        dirty_ = true;
    }

    void TextBlock::set_font(RefCountPtr<FontFace> font) noexcept
    {
        font_ = std::move(font);
        dirty_ = true;
    }

    void TextBlock::set_pixel_size(const std::uint32_t pixel_size) noexcept
    {
        pixel_size_ = pixel_size;
    }

    void TextBlock::set_pivot(const Vector2f pivot) noexcept
    {
        pivot_ = pivot;
        dirty_ = true;
    }

    void TextBlock::on_world_transform_updated()
    {
        SceneNode::on_world_transform_updated();
        dirty_ = true;
    }

    TextBlockRenderPipeline::TextBlockRenderPipeline(RenderBackend &render_backend) : font_atlas_cache_{render_backend}
    {
    }

    std::type_index TextBlockRenderPipeline::component_type() const
    {
        return typeid(TextBlock);
    }

    const ShaderLayout &TextBlockRenderPipeline::shaders() const
    {
        static const ShaderLayout layout{
            .vertex_shader = shaders::text_vert,
            .fragment_shader = shaders::text_frag,
            .vertex_bindings = {VertexInputBinding{
                .type = VertexInputType::instance,
                .stride = sizeof(TextBlockInstanceData),
                .attributes =
                    {
                        // Vulkan doesn't have matrix attributes, so the most compatible option is to
                        // just treat a matrix as an array of vectors
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, transform)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, transform) + sizeof(Vector2f)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, translation)},
                        VertexAttribute{.type = ShaderDataType::int32,
                                        .size = sizeof(std::int32_t),
                                        .offset = offsetof(TextBlockInstanceData, z_order)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, pivot)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, size)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, min_uv)},
                        VertexAttribute{.type = ShaderDataType::vec2,
                                        .size = sizeof(Vector2f),
                                        .offset = offsetof(TextBlockInstanceData, max_uv)},
                        VertexAttribute{.type = ShaderDataType::vec4,
                                        .size = sizeof(Color),
                                        .offset = offsetof(TextBlockInstanceData, tint)},
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

    SmallUniquePtr<DrawCommandSource> TextBlockRenderPipeline::collect_draw_calls_source(
        const SceneNodeList &nodes,
        const Vector2u viewport_size,
        const Viewport &viewport,
        std::pmr::memory_resource &memory_resource)
    {
        std::pmr::unordered_map<const GpuFontAtlas *, TextBlockBatch> batches{&memory_resource};
        for (auto *node : nodes.nodes_of_type<TextBlock>())
        {
            FontSdfConfig config{.pixel_size = node->pixel_size()};
            auto font_atlas = font_atlas_cache_.get_or_create(*node->font(), config);

            update_cached_quads(*node, *font_atlas);

            auto pending_data = node->cached_quads_ | std::views::transform(
                                                          [node](const TextQuad &quad)
                                                          {
                                                              return TextBlockInstanceData{
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

            if (auto it = batches.find(font_atlas.get()); it == batches.end())
            {
                auto [pair, inserted] =
                    batches.emplace(font_atlas.get(),
                                    TextBlockBatch{
                                        .font_atlas = font_atlas,
                                        .instances = std::pmr::vector<TextBlockInstanceData>{&memory_resource},
                                        .viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size),
                                    });

                pair->second.instances.append_range(pending_data);
            }
            else
            {
                auto &[draw_texture, instances, viewport_draw_info] = batches[font_atlas.get()];
                draw_texture = font_atlas;
                viewport_draw_info = viewport.camera_layout().get_draw_info(viewport_size);
                instances.append_range(pending_data);
            }
        }

        return DrawCommandSource::from(std::move(batches) | std::views::values |
                                       std::ranges::to<std::pmr::vector<TextBlockBatch>>(&memory_resource));
    }

    void TextBlockRenderPipeline::update_cached_quads(TextBlock &text_block, const GpuFontAtlas &font_atlas)
    {
        if (!text_block.dirty_)
            return;

        text_block.cached_quads_.clear();

        const auto codepoints = convert_string<char32_t>(text_block.text());
        text_block.cached_quads_.reserve(codepoints.size());

        std::vector<PendingGlyphQuad> pending_quads;
        pending_quads.reserve(codepoints.size());

        const auto &font_metrics = font_atlas.metrics();

        Vector2f pen{};
        Vector2f min_bounds{std::numeric_limits<float>::max(), std::numeric_limits<float>::max()};

        Vector2f max_bounds{std::numeric_limits<float>::lowest(), std::numeric_limits<float>::lowest()};

        bool has_bounds = false;

        for (const auto codepoint : codepoints)
        {
            if (codepoint == U'\r')
                continue;

            if (codepoint == U'\n')
            {
                pen.x = 0.0f;
                pen.y += font_metrics.line_height;
                continue;
            }

            const auto glyph = font_atlas.find_glyph(codepoint);
            if (!glyph.has_value())
                continue;

            const auto &glyph_metrics = *glyph;

            const auto glyph_position = Vector2f{
                pen.x + glyph_metrics.bearing_x,
                pen.y + font_metrics.ascender - glyph_metrics.bearing_y,
            };

            const auto glyph_size = Vector2f{
                glyph_metrics.width,
                glyph_metrics.height,
            };

            if (glyph_size.x > 0.0f && glyph_size.y > 0.0f)
            {
                pending_quads.push_back({.position = glyph_position,
                                         .size = glyph_size,
                                         .uvs = UVs{
                                             .min = {glyph_metrics.uv_min_x, glyph_metrics.uv_min_y},
                                             .max = {glyph_metrics.uv_max_x, glyph_metrics.uv_max_y},
                                         }});

                const auto glyph_min = glyph_position;
                const auto glyph_max = glyph_position + glyph_size;

                if (!has_bounds)
                {
                    min_bounds = glyph_min;
                    max_bounds = glyph_max;
                    has_bounds = true;
                }
                else
                {
                    min_bounds.x = std::min(min_bounds.x, glyph_min.x);
                    min_bounds.y = std::min(min_bounds.y, glyph_min.y);
                    max_bounds.x = std::max(max_bounds.x, glyph_max.x);
                    max_bounds.y = std::max(max_bounds.y, glyph_max.y);
                }
            }

            pen.x += glyph_metrics.advance_x;
        }

        if (!has_bounds)
        {
            text_block.dirty_ = false;
            return;
        }

        const auto bounds_size = max_bounds - min_bounds;
        const auto pivot_offset = min_bounds + bounds_size * text_block.pivot_;

        const auto world_matrix = text_block.world_transform().matrix();
        const auto world_translation = text_block.world_transform().translation();

        for (const auto &quad : pending_quads)
        {
            const auto local_position = quad.position - pivot_offset;
            const auto transformed_position = world_matrix * local_position + world_translation;

            text_block.cached_quads_.push_back({.transform = Transform2f{transformed_position},
                                                .uvs = quad.uvs,
                                                .pivot = Vector2f::zero(),
                                                .size = quad.size});
        }

        text_block.dirty_ = false;
    }
} // namespace retro
