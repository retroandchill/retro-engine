/**
 * @file geometry.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.objects.geometry;

import std;
import retro.core.util.color;
import retro.core.math.matrix;
import retro.core.math.vector;
import retro.runtime.rendering.draw_command;
import retro.runtime.rendering.render_pipeline;
import retro.runtime.rendering.shader_layout;
import retro.runtime.world.scene;
import retro.runtime.world.scene_node;

namespace retro
{
    export class GeometryRenderPipeline;

    export enum class GeometryType : std::uint8_t
    {
        None,
        Rectangle,
        Triangle,
        Custom
    };

    export struct Vertex
    {
        Vector2f position{};
        Vector2f uv{};
    };

    export struct Geometry
    {
        std::vector<Vertex> vertices{};
        std::vector<std::uint32_t> indices{};
    };

    export struct GeometryInstanceData
    {
        alignas(16) Matrix2x2f transform{};
        alignas(8) Vector2f translation{};
        alignas(8) Vector2f pivot{};
        alignas(8) Vector2f size{1, 1};
        alignas(16) Color color{1, 1, 1, 1};
        std::uint32_t has_texture{};
    };

    export struct GeometryBatch
    {
        const Geometry *geometry{};
        std::vector<GeometryInstanceData> instances{};
        std::uint32_t texture_handle{};
        Vector2f viewport_size{};

        DrawCommand create_draw_command() const;
    };

    export class GeometryObject final : public SceneNode
    {
      public:
        using PipelineType = GeometryRenderPipeline;

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

        void collect_draw_calls(const SceneNodeList &nodes, Vector2u viewport_size) override;

        void execute(RenderContext &context) override;

      private:
        std::unordered_map<const Geometry *, GeometryBatch> geometry_batches_{};
    };
} // namespace retro
