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
import :entt;

namespace retro
{
    export struct Vertex
    {
        Vector2f position{};
        Vector2f uv{};
        Color color{};
    };

    export template <template <typename...> typename Alloc>
        requires SimpleAllocator<Alloc<Vertex>> && SimpleAllocator<Alloc<uint32>>
    struct BasicGeometry
    {
        std::vector<Vertex, Alloc<Vertex>> vertices{};
        std::vector<uint32, Alloc<uint32>> indices{};

        constexpr explicit BasicGeometry(Alloc<Vertex> vertex_allocator = Alloc<Vertex>{},
                                         Alloc<uint32> index_allocator = Alloc<uint32>{})
            : vertices{std::move(vertex_allocator)}, indices{std::move(index_allocator)}
        {
        }

        constexpr BasicGeometry(std::span<const Vertex> vertices,
                                std::span<const uint32> indices,
                                Alloc<Vertex> vertex_allocator = Alloc<Vertex>{},
                                Alloc<uint32> index_allocator = Alloc<uint32>{}) noexcept
            : vertices{std::from_range, vertices, std::move(vertex_allocator)},
              indices{std::from_range, indices, std::move(index_allocator)}
        {
        }

        constexpr BasicGeometry(const BasicGeometry &) = default;
        constexpr BasicGeometry(BasicGeometry &&) noexcept = default;

        template <template <typename...> typename OtherAlloc>
            requires SimpleAllocator<OtherAlloc<Vertex>> && SimpleAllocator<OtherAlloc<uint32>>
        constexpr explicit BasicGeometry(const BasicGeometry<OtherAlloc> &other,
                                         Alloc<Vertex> vertex_allocator = Alloc<Vertex>{},
                                         Alloc<uint32> index_allocator = Alloc<uint32>{})
            : vertices{std::from_range, other.vertices, std::move(vertex_allocator)},
              indices{std::from_range, other.indices, std::move(index_allocator)}
        {
        }

        ~BasicGeometry() = default;

        constexpr BasicGeometry &operator=(const BasicGeometry &) = default;
        constexpr BasicGeometry &operator=(BasicGeometry &&) noexcept = default;
    };

    export template <template <typename...> typename Alloc>
    BasicGeometry(std::span<Vertex>, std::span<uint32>, Alloc<Vertex>, Alloc<uint32>) -> BasicGeometry<Alloc>;

    export template <template <typename...> typename Alloc, template <typename...> typename OtherAlloc>
    BasicGeometry(const BasicGeometry<OtherAlloc> &, Alloc<Vertex>, Alloc<uint32>) -> BasicGeometry<Alloc>;

    export using Geometry = BasicGeometry<std::allocator>;

    export struct GeometryDrawCall
    {
        BasicGeometry<ArenaAllocator> geometry;
        std::vector<std::byte, ArenaAllocator<std::byte>> push_constants;

        template <template <typename...> typename OtherAlloc>
            requires SimpleAllocator<OtherAlloc<Vertex>> && SimpleAllocator<OtherAlloc<uint32>>
        constexpr GeometryDrawCall(SingleArena &arena,
                                   const BasicGeometry<OtherAlloc> &other,
                                   const usize push_constants_size = 0)
            : geometry{other, make_allocator<Vertex>(arena), make_allocator<uint32>(arena)},
              push_constants{push_constants_size, make_allocator<std::byte>(arena)}
        {
        }
    };

    export struct ProceduralDrawCall
    {
        uint32 vertex_count;
        std::vector<std::byte, ArenaAllocator<std::byte>> push_constants;

        template <template <typename...> typename OtherAlloc>
            requires SimpleAllocator<OtherAlloc<Vertex>> && SimpleAllocator<OtherAlloc<uint32>>
        constexpr ProceduralDrawCall(SingleArena &arena, const uint32 vertex_count, const usize push_constants_size = 0)
            : vertex_count{vertex_count}, push_constants{push_constants_size, make_allocator<std::byte>(arena)}
        {
        }
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

        virtual void collect_draw_calls(entt::registry &registry, Vector2u viewport_size, SingleArena &arena) = 0;

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
        static constexpr usize DEFAULT_POOL_SIZE = 1024 * 1024 * 16;

        explicit PipelineManager(Renderer2D &renderer) : renderer_{&renderer}
        {
        }

        template <RenderComponent Component>
        void set_up_pipeline_listener(entt::registry &registry)
        {
            registry.on_construct<Component>().template connect<&PipelineManager::on_component_added<Component>>(this);
            registry.on_destroy<Component>().template connect<&PipelineManager::on_component_removed<Component>>(this);
        }

        void reset_arena();

        void collect_all_draw_calls(entt::registry &registry, Vector2u viewport_size);

      private:
        template <RenderComponent Component>
        void on_component_added(entt::registry &, entt::entity)
        {
            using Pipeline = Component::PipelineType;
            auto existing_pipeline = pipelines_.find(std::type_index{typeid(Component)});
            if (existing_pipeline == pipelines_.end())
            {
                existing_pipeline =
                    pipelines_
                        .emplace(std::type_index{typeid(Component)}, PipelineUsage{std::make_shared<Pipeline>(), 0})
                        .first;
            }

            if (const std::type_index type_index{typeid(Component)}; existing_pipeline->second.usage_count++ == 0)
            {
                renderer_->add_new_render_pipeline(type_index, existing_pipeline->second.pipeline);
            }
        }

        template <RenderComponent Component>
        void on_component_removed(entt::registry &, entt::entity)
        {
            const auto existing_pipeline = pipelines_.find(std::type_index{typeid(Component)});
            if (existing_pipeline == pipelines_.end())
            {
                get_logger().warn("No pipeline found for component type {}", typeid(Component).name());
                return;
            }

            if (const std::type_index type_index{typeid(Component)}; --existing_pipeline->second.usage_count == 0)
            {
                renderer_->remove_render_pipeline(type_index);
            }
        }

        Renderer2D *renderer_{};
        std::map<std::type_index, PipelineUsage> pipelines_{};
        SingleArena arena_{DEFAULT_POOL_SIZE};
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
            registrations_.emplace_back([this](entt::registry &registry, PipelineManager &pipeline_manager)
                                        { pipeline_manager.set_up_pipeline_listener<T>(registry); });
        }

        void register_listeners(entt::registry &registry, PipelineManager &pipeline_manager) const;

      private:
        std::vector<std::function<void(entt::registry &, PipelineManager &)>> registrations_{};
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
