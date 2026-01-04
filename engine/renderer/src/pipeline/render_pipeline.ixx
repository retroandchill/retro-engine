/**
 * @file render_pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:pipeline.render_pipeline;

import std;
import vulkan_hpp;
import :components;

namespace retro
{
    export class RETRO_API RenderPipeline
    {
      public:
        virtual ~RenderPipeline() = default;

        virtual Name type() const = 0;

        virtual void clear_draw_queue() = 0;

        virtual void recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass) = 0;

        virtual void queue_draw_calls(const std::any &render_data) = 0;

        virtual void bind_and_render(vk::CommandBuffer cmd, Vector2u viewport_size) = 0;
    };
} // namespace retro
