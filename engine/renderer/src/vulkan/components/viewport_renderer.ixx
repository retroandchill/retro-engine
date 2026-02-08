/**
 * @file viewport_renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.viewport_renderer;

import vulkan_hpp;
import retro.core.di;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.pipeline;

namespace retro
{
    export struct ViewportConfig
    {
        std::uint32_t x = 0;
        std::uint32_t y = 0;
        std::uint32_t width;
        std::uint32_t height;
        std::uint32_t frames_in_flight = 2;
    };

    export class ViewportRenderer
    {
      public:
        using Dependencies =
            TypeList<VulkanDevice, vk::SurfaceKHR, VulkanSwapchain, VulkanBufferManager, VulkanPipelineManager>;

        ViewportRenderer(const ViewportConfig &config,
                         VulkanDevice &device,
                         vk::SurfaceKHR surface,
                         const VulkanSwapchain &swapchain,
                         VulkanBufferManager &buffer_manager,
                         VulkanPipelineManager &pipeline_manager);
    };
} // namespace retro
