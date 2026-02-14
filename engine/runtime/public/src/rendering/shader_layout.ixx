/**
 * @file shader_layout.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.shader_layout;

import std;
import retro.core.containers.optional;

namespace retro
{
    export enum class ShaderDataType : std::uint8_t
    {
        int32,
        uint32,
        float32,
        vec2,
        vec3,
        vec4
    };

    export enum class ShaderStage : std::uint8_t
    {
        vertex = 0x1,
        fragment = 0x2
    };

    export constexpr ShaderStage operator&(ShaderStage lhs, ShaderStage rhs) noexcept
    {
        return static_cast<ShaderStage>(static_cast<std::uint8_t>(lhs) & static_cast<std::uint8_t>(rhs));
    }

    export constexpr ShaderStage operator|(ShaderStage lhs, ShaderStage rhs) noexcept
    {
        return static_cast<ShaderStage>(static_cast<std::uint8_t>(lhs) | static_cast<std::uint8_t>(rhs));
    }

    export constexpr bool has_flag(const ShaderStage target, const ShaderStage flag) noexcept
    {
        return static_cast<std::uint8_t>(target & flag) != 0;
    }

    export enum class VertexInputType : std::uint8_t
    {
        vertex,
        instance
    };

    export struct VertexAttribute
    {
        ShaderDataType type{};
        std::size_t size{};
        std::size_t offset{};
    };

    export struct VertexInputBinding
    {
        VertexInputType type{};
        std::size_t stride{};
        std::vector<VertexAttribute> attributes{};
    };

    export enum class DescriptorType : std::uint8_t
    {
        sampler,
        combined_image_sampler,
        uniform_buffer,
        storage_buffer
    };

    export struct DescriptorBinding
    {
        DescriptorType type{};
        ShaderStage stages{};
        std::size_t count{};
    };

    export struct PushConstantBinding
    {
        ShaderStage stages{};
        std::size_t size{};
        std::size_t offset{};
    };

    export struct ShaderLayout
    {
        std::filesystem::path vertex_shader{};
        std::filesystem::path fragment_shader{};
        std::vector<VertexInputBinding> vertex_bindings{};
        std::vector<DescriptorBinding> descriptor_bindings{};
        Optional<PushConstantBinding> push_constant_bindings{};
    };
} // namespace retro
