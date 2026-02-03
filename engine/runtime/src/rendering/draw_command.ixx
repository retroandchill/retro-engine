/**
 * @file draw_command.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.draw_command;

import std;
import retro.runtime.rendering.texture_render_data;
import retro.core.containers.inline_list;

namespace retro
{
    export constexpr std::size_t DRAW_ARRAY_SIZE = 8;

    export using DescriptorSetData = std::variant<std::span<const std::byte>, const TextureRenderData *>;

    export struct DrawCommand
    {
        InlineList<std::span<const std::byte>, DRAW_ARRAY_SIZE> vertex_buffers{};
        InlineList<std::span<const std::byte>, DRAW_ARRAY_SIZE> instance_buffers{};
        std::span<const std::byte> index_buffer;
        InlineList<DescriptorSetData, DRAW_ARRAY_SIZE> descriptor_sets{};
        std::span<const std::byte> push_constants;
        std::size_t index_count{};
        std::size_t instance_count{};
    };
} // namespace retro
