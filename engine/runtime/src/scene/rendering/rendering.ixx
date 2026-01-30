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
    export enum class ShaderDataType : uint8
    {
        Int32,
        Uint32,
        Float,
        Vec2,
        Vec3,
        Vec4
    };

    export enum class ShaderStage : uint8
    {
        Vertex = 0x1,
        Fragment = 0x2
    };

    export constexpr ShaderStage operator&(ShaderStage lhs, ShaderStage rhs) noexcept
    {
        return static_cast<ShaderStage>(static_cast<uint8>(lhs) & static_cast<uint8>(rhs));
    }

    export constexpr ShaderStage operator|(ShaderStage lhs, ShaderStage rhs) noexcept
    {
        return static_cast<ShaderStage>(static_cast<uint8>(lhs) | static_cast<uint8>(rhs));
    }

    export constexpr bool has_flag(const ShaderStage target, const ShaderStage flag) noexcept
    {
        return static_cast<uint8>(target & flag) != 0;
    }

    export enum class VertexInputType : uint8
    {
        Vertex,
        Instance
    };

    export struct VertexAttribute
    {
        ShaderDataType type{};
        usize size{};
        usize offset{};
    };

    export struct VertexInputBinding
    {
        VertexInputType type{};
        usize stride{};
        std::vector<VertexAttribute> attributes{};
    };

    export enum class DescriptorType : uint8
    {
        Sampler,
        CombinedImageSampler,
        UniformBuffer,
        StorageBuffer
    };

    export struct DescriptorBinding
    {
        DescriptorType type{};
        ShaderStage stages{};
        usize count{};
    };

    export struct PushConstantBinding
    {
        ShaderStage stages{};
        usize size{};
        usize offset{};
    };

    export struct ShaderLayout
    {
        std::filesystem::path vertex_shader{};
        std::filesystem::path fragment_shader{};
        std::vector<VertexInputBinding> vertex_bindings{};
        std::vector<DescriptorBinding> descriptor_bindings{};
        std::vector<PushConstantBinding> push_constant_bindings{};
    };

    export struct Vertex
    {
        Vector2f position{};
        Vector2f uv{};
    };

    export struct Geometry
    {
        std::vector<Vertex> vertices{};
        std::vector<uint32> indices{};
    };

    export struct InstanceData
    {
        alignas(16) Matrix2x2f transform{};
        alignas(8) Vector2f translation{};
        alignas(8) Vector2f pivot{};
        alignas(8) Vector2f size{1, 1};
        alignas(16) Color color{1, 1, 1, 1};
        uint32 has_texture{};
    };

    export struct GeometryBatch
    {
        const Geometry *geometry{};
        std::vector<InstanceData> instances{};
        uint32 texture_handle{};
        Vector2f viewport_size{};
    };

    export class RenderContext
    {
      public:
        virtual ~RenderContext() = default;

        virtual void draw_geometry(std::span<const GeometryBatch> geometry) = 0;
    };

    export class RenderPipeline
    {
      public:
        virtual ~RenderPipeline() = default;

        [[nodiscard]] virtual std::type_index component_type() const = 0;

        [[nodiscard]] virtual const ShaderLayout &shaders() const = 0;

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

    struct PipelineUsage
    {
        std::shared_ptr<RenderPipeline> pipeline;
        usize usage_count;
    };

    export class RETRO_API PipelineManager
    {
      public:
        using Dependencies = TypeList<Renderer2D, RenderPipeline>;
        static constexpr usize DEFAULT_POOL_SIZE = 1024 * 1024 * 16;

        explicit PipelineManager(Renderer2D &renderer, const std::vector<std::shared_ptr<RenderPipeline>> &pipelines);

        void collect_all_draw_calls(Scene &registry, Vector2u viewport_size);

      private:
        Renderer2D *renderer_{};
        std::map<std::type_index, PipelineUsage> pipelines_{};
    };

} // namespace retro
