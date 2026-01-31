/**
 * @file sprite.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.sprite;

import :texture;
import :scene;
import :scene.rendering;

namespace retro
{
    export class SpriteRenderPipeline;

    export struct SpriteInstanceData
    {
        alignas(16) Matrix2x2f transform{};
        alignas(8) Vector2f translation{};
        alignas(8) Vector2f pivot{};
        alignas(8) Vector2f size{1, 1};
        alignas(8) Vector2f min_uv{0, 0};
        alignas(8) Vector2f max_uv{1, 1};
        alignas(16) Color tint{1, 1, 1, 1};
    };

    export struct SpriteBatch
    {
        const Texture *texture = nullptr;
        std::vector<SpriteInstanceData> instances;
        Vector2f viewport_size{};

        [[nodiscard]] DrawCommand create_draw_command() const;
    };

    export class Sprite final : public SceneNode
    {
      public:
        using PipelineType = SpriteRenderPipeline;

        inline explicit Sprite(Scene &scene) : SceneNode(scene)
        {
        }

        [[nodiscard]] inline const RefCountPtr<Texture> &texture() const noexcept
        {
            return texture_;
        }

        inline void set_texture(RefCountPtr<Texture> texture) noexcept
        {
            texture_ = std::move(texture);
        }

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

        inline void set_pivot(const Vector2f pivot) noexcept
        {
            pivot_ = pivot;
        }

        [[nodiscard]] inline Vector2f size() const noexcept
        {
            return size_;
        }

        inline void set_size(const Vector2f size) noexcept
        {
            size_ = size;
        }

      private:
        RefCountPtr<Texture> texture_;
        Color tint_ = Color::white();
        Vector2f pivot_{};
        Vector2f size_{100, 100};
    };

    class RETRO_API SpriteRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] std::type_index component_type() const override;

        [[nodiscard]] const ShaderLayout &shaders() const override;

        void clear_draw_queue() override;

        void collect_draw_calls(Scene &registry, Vector2u viewport_size) override;

        void execute(RenderContext &context) override;

      private:
        std::unordered_map<Texture *, SpriteBatch> batches_;
    };
} // namespace retro
