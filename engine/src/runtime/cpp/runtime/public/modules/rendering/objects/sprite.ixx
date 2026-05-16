/**
 * @file sprite.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.objects.sprite;

import std;
import retro.core.util.color;
import retro.core.math.matrix;
import retro.core.math.vector;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.draw_command;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.shader_layout;
import retro.runtime.world.scene;
import retro.runtime.world.viewport;
import retro.runtime.world.scene_node;
import retro.core.memory.small_unique_ptr;
import retro.runtime.rendering.texture;
import retro.runtime.rendering.layout.margin;
import retro.core.math.transform;

namespace retro
{
    export struct UVs
    {
        Vector2f min{0, 0};
        Vector2f max{1, 1};
    };

    export enum class SpriteDrawMode : std::uint8_t
    {
        quad,
        box
    };

    export class SpriteRenderPipeline;

    export struct SpriteInstanceData
    {
        alignas(16) Matrix2x2f transform{};
        alignas(8) Vector2f translation{};
        std::int32_t z_order{};
        alignas(8) Vector2f pivot{};
        alignas(8) Vector2f size{1, 1};
        alignas(8) Vector2f min_uv{0, 0};
        alignas(8) Vector2f max_uv{1, 1};
        alignas(16) Color tint{1, 1, 1, 1};
    };

    export class Sprite;

    export struct SpriteBatch
    {
        using ComponentType = Sprite;

        const Texture *texture = nullptr;
        std::pmr::vector<SpriteInstanceData> instances;
        ViewportDrawInfo viewport_draw_info{};

        [[nodiscard]] DrawCommand create_draw_command() const;
    };

    struct SpriteQuad
    {
        Transform2f transform{};
        UVs uvs{};
        Vector2f pivot{};
        Vector2f size{100, 100};
    };

    class RETRO_API Sprite final : public SceneNode
    {
      public:
        using PipelineType = SpriteRenderPipeline;

        [[nodiscard]] inline const RefCountPtr<Texture> &texture() const noexcept
        {
            return texture_;
        }

        void set_texture(RefCountPtr<Texture> texture) noexcept;

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

        [[nodiscard]] inline Vector2f size() const noexcept
        {
            return size_;
        }

        void set_size(Vector2f size) noexcept;

        [[nodiscard]] inline const UVs &uvs() const noexcept
        {
            return uvs_;
        }

        void set_uvs(const UVs &uvs) noexcept;

        [[nodiscard]] inline SpriteDrawMode draw_mode() const noexcept
        {
            return draw_mode_;
        }

        void set_draw_mode(SpriteDrawMode draw_mode) noexcept;

        [[nodiscard]] inline const Margin &margin() const noexcept
        {
            return margin_;
        }

        void set_margin(const Margin &margin) noexcept;

      protected:
        void on_world_transform_updated() override;

      private:
        friend class SpriteRenderPipeline;

        void mark_cached_render_data_as_dirty();
        void refresh_cached_render_data();

        RefCountPtr<Texture> texture_;
        Color tint_ = Color::white();
        Vector2f pivot_{};
        Vector2f size_{100, 100};
        UVs uvs_{};
        SpriteDrawMode draw_mode_ = SpriteDrawMode::quad;
        Margin margin_{};
        std::vector<SpriteQuad> cached_quads_;
        bool render_data_dirty_{true};
    };

    class RETRO_API SpriteRenderPipeline final : public RenderPipeline
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
