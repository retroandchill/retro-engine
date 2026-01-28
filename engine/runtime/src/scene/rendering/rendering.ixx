/**
 * @file rendering.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.rendering;

import retro.core;
import retro.logging;
import std;

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
        std::filesystem::path vertex_shader{};
        std::filesystem::path fragment_shader{};
    };

    export class RenderPipeline
    {
      public:
        virtual ~RenderPipeline() = default;

        [[nodiscard]] virtual std::type_index component_type() const = 0;

        [[nodiscard]] virtual usize push_constants_size() const = 0;

        [[nodiscard]] virtual PipelineShaders shaders() const = 0;

        virtual void clear_draw_queue() = 0;

        virtual void collect_draw_calls(class Scene &registry, Vector2u viewport_size) = 0;

        virtual void execute(RenderContext &context) = 0;
    };

    export class Renderer2D
    {
      public:
        virtual ~Renderer2D() = default;

        virtual void begin_frame() = 0;

        virtual void end_frame() = 0;

        virtual void add_new_render_pipeline(std::type_index type, std::shared_ptr<RenderPipeline> pipeline) = 0;

        virtual void remove_render_pipeline(std::type_index type) = 0;

        [[nodiscard]] virtual Vector2u viewport_size() const = 0;
    };

    export template <typename T>
    concept RenderComponent =
        requires { typename T::PipelineType; } && std::is_default_constructible_v<typename T::PipelineType> &&
        std::derived_from<typename T::PipelineType, RenderPipeline>;

    struct PipelineUsage
    {
        std::shared_ptr<RenderPipeline> pipeline;
        usize usage_count;
    };

    export class RETRO_API PipelineManager
    {
      public:
        explicit PipelineManager(Renderer2D &renderer) : renderer_{&renderer}
        {
        }

        template <RenderComponent Component>
        void set_up_pipeline_listener()
        {
            using Pipeline = Component::PipelineType;
            const std::type_index type_index{typeid(Component)};

            auto it = pipelines_.find(type_index);
            if (it == pipelines_.end())
            {
                it = pipelines_.emplace(type_index, PipelineUsage{std::make_shared<Pipeline>(), 0}).first;
            }

            // For OO: keep pipelines around; the renderer needs them created once.
            if (it->second.usage_count++ == 0)
            {
                renderer_->add_new_render_pipeline(type_index, it->second.pipeline);
            }
        }

        void collect_all_draw_calls(Scene &registry, Vector2u viewport_size);

      private:
        Renderer2D *renderer_{};
        std::map<std::type_index, PipelineUsage> pipelines_{};
    };

    export class RETRO_API RenderTypeRegistry
    {
        RenderTypeRegistry() = default;
        ~RenderTypeRegistry() = default;

      public:
        static RenderTypeRegistry &instance();

        template <RenderComponent T>
        void register_type()
        {
            registrations_.emplace_back([this](PipelineManager &pipeline_manager)
                                        { pipeline_manager.set_up_pipeline_listener<T>(); });
        }

        void register_listeners(PipelineManager &pipeline_manager) const;

      private:
        std::vector<std::function<void(PipelineManager &)>> registrations_{};
    };

    export template <RenderComponent T>
    struct PipelineRegistration
    {
        explicit PipelineRegistration()
        {
            RenderTypeRegistry::instance().register_type<T>();
        }
    };

} // namespace retro
