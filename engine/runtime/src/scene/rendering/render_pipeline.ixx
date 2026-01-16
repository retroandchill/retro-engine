/**
 * @file render_pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.rendering.render_pipeline;

import std;
import retro.core;

namespace retro
{
    export struct Vertex
    {
        Vector2f position{};
        Vector2f uv{};
        Color color{};
    };

    export struct Geometry
    {
        std::vector<Vertex> vertices{};
        std::vector<uint32> indices{};
    };

    export struct GeometryDrawCall
    {
        Geometry geometry{};
        std::vector<std::byte> push_constants{};
    };

    export struct ProceduralDrawCall
    {
        uint32 vertex_count;
        std::vector<std::byte> push_constants;
    };

    export class RenderContext
    {
      public:
        virtual ~RenderContext() = default;

        virtual void draw_geometry(std::span<const GeometryDrawCall> geometry) = 0;

        virtual void draw_procedural(std::span<ProceduralDrawCall> vertex_count) = 0;
    };

    export struct PipelineShaders
    {
        std::filesystem::path vertex_shader;
        std::filesystem::path fragment_shader;
    };

    export class RenderPipeline
    {
      public:
        virtual ~RenderPipeline() = default;

        [[nodiscard]] virtual Name type() const = 0;

        [[nodiscard]] virtual usize push_constants_size() const = 0;

        [[nodiscard]] virtual PipelineShaders shaders() const = 0;

        virtual void clear_draw_queue() = 0;

        virtual void queue_draw_calls(const std::any &render_data) = 0;

        virtual void execute(RenderContext &context) = 0;
    };
} // namespace retro
