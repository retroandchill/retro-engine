/**
 * @file geometry.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering.geometry;

import std;
import retro.core;
import :scene;

namespace retro
{
    export class GeometryRenderPipeline;

    export enum class GeometryType : uint8
    {
        None,
        Rectangle,
        Triangle,
        Custom
    };

    export class GeometryObject final : public SceneNode
    {
      public:
        using PipelineType = GeometryRenderPipeline;

        inline explicit GeometryObject(Scene &scene) : SceneNode(scene)
        {
        }

        [[nodiscard]] inline const std::shared_ptr<const Geometry> &geometry() const noexcept
        {
            return geometry_;
        }

        inline void set_geometry(std::shared_ptr<const Geometry> geometry)
        {
            geometry_ = std::move(geometry);
        }

        void set_geometry(GeometryType type);

        [[nodiscard]] inline Color color() const noexcept
        {
            return color_;
        }

        inline void set_color(const Color color) noexcept
        {
            color_ = color;
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
        std::shared_ptr<const Geometry> geometry_;
        Color color_ = Color::white();
        Vector2f pivot_{};
        Vector2f size_{100, 100};
    };

    class RETRO_API GeometryRenderPipeline final : public RenderPipeline
    {
      public:
        [[nodiscard]] std::type_index component_type() const override;

        [[nodiscard]] const ShaderLayout &shaders() const override;

        void clear_draw_queue() override;

        void collect_draw_calls(Scene &registry, Vector2u viewport_size) override;

        void execute(RenderContext &context) override;

      private:
        std::unordered_map<const Geometry *, GeometryBatch> geometry_batches_{};
    };
} // namespace retro
