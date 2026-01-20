/**
 * @file render_pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.rendering.render_pipeline;

import std;
import retro.core;
import entt;

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
            : geometry(other, make_allocator<Vertex>(arena), make_allocator<uint32>(arena)),
              push_constants(push_constants_size, make_allocator<std::byte>(arena))
        {
        }
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

        [[nodiscard]] virtual usize push_constants_size() const = 0;

        [[nodiscard]] virtual PipelineShaders shaders() const = 0;

        virtual void clear_draw_queue() = 0;

        virtual void collect_draw_calls(const entt::registry &registry, Vector2u viewport_size, SingleArena &arena) = 0;

        virtual void execute(RenderContext &context) = 0;
    };

    export template <typename T>
    concept RenderComponent =
        requires { typename T::PipelineType; } && std::is_default_constructible_v<typename T::PipelineType> &&
        std::derived_from<typename T::PipelineType, RenderPipeline>;
} // namespace retro
