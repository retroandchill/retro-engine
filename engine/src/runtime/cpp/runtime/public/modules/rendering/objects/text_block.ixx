/**
 * @file text_block.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.objects.text_block;

import retro.runtime.world.scene_node;

import std;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.text.font;
import retro.core.util.color;
import retro.core.math.vector;
import retro.core.math.matrix;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.shader_layout;
import retro.core.memory.small_unique_ptr;
import retro.runtime.rendering.draw_command;
import retro.runtime.world.viewport;
import retro.runtime.rendering.texture;
import retro.runtime.rendering.layout.uvs;
import retro.core.math.transform;
import retro.runtime.rendering.render_backend;
import retro.core.containers.optional;

namespace retro
{
    export class TextBlockRenderPipeline;

    export struct TextBlockInstanceData
    {
        alignas(16) Matrix2x2f transform{};
        alignas(8) Vector2f translation{};
        std::int32_t z_order{};
        alignas(8) Vector2f pivot{};
        alignas(8) Vector2f size{1, 1};
        alignas(8) Vector2f min_uv{0, 0};
        alignas(8) Vector2f max_uv{1, 1};
        alignas(16) Color tint{1, 1, 1, 1};
        float pixel_range{};
    };

    export class TextBlock;

    export struct TextBlockBatch
    {
        using ComponentType = TextBlock;

        RefCountPtr<Texture> font_texture = nullptr;
        std::pmr::vector<TextBlockInstanceData> instances;
        ViewportDrawInfo viewport_draw_info{};

        [[nodiscard]] DrawCommand create_draw_command() const;
    };

    struct TextQuad
    {
        Transform2f transform{};
        UVs uvs{};
        Vector2f pivot{};
        Vector2f size{};
    };

    class TextBlock final : public SceneNode
    {
      public:
        [[nodiscard]] inline std::string_view text() const noexcept
        {
            return text_;
        }

        void set_text(std::string text) noexcept;

        [[nodiscard]] inline const RefCountPtr<Font> &font() const noexcept
        {
            return font_;
        }

        void set_font(RefCountPtr<Font> font) noexcept;

        [[nodiscard]] inline std::uint32_t pixel_size() const noexcept
        {
            return pixel_size_;
        }

        void set_pixel_size(std::uint32_t pixel_size) noexcept;

        [[nodiscard]] inline Color tint() const noexcept
        {
            return tint_;
        }

        inline void set_tint(const Color tint) noexcept
        {
            tint_ = tint;
        }

        [[nodiscard]] inline Vector2f pivot() const noexcept
        {
            return pivot_;
        }

        void set_pivot(Vector2f pivot) noexcept;

      protected:
        void on_world_transform_updated() override;

      private:
        friend class TextBlockRenderPipeline;

        Optional<const FontAtlas &> refresh_cached_quads();

        std::string text_;
        RefCountPtr<Font> font_;
        std::uint32_t pixel_size_{48};
        Color tint_{Color::white()};
        Vector2f pivot_{};
        bool dirty_{true};
        std::vector<TextQuad> cached_quads_;
    };

    class RETRO_API TextBlockRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] std::type_index component_type() const override;
        [[nodiscard]] const ShaderLayout &shaders() const override;
        SmallUniquePtr<DrawCommandSource> collect_draw_calls_source(
            const SceneNodeList &nodes,
            Vector2u viewport_size,
            const Viewport &viewport,
            std::pmr::memory_resource &memory_resource) override;
    };
} // namespace retro
