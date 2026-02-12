/**
 * @file viewport_renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.viewport_renderer;

import vulkan_hpp;
import retro.core.di;
import retro.core.math.vector;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.pipeline;
import retro.runtime.world.viewport;

namespace retro
{
    export class ViewportRenderer
    {
      public:
        ViewportRenderer(const Viewport &viewport,
                         VulkanDevice &device,
                         vk::SurfaceKHR surface,
                         const VulkanSwapchain &swapchain,
                         VulkanBufferManager &buffer_manager,
                         VulkanPipelineManager &pipeline_manager);

        [[nodiscard]] inline const Viewport &viewport() const noexcept
        {
            return viewport_;
        }

        void render_viewport(vk::CommandBuffer cmd,
                             const Vector2u &framebuffer_size,
                             vk::DescriptorPool descriptor_pool);

      private:
        const Viewport &viewport_;
        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        const VulkanSwapchain &swapchain_;
        VulkanBufferManager &buffer_manager_;
        VulkanPipelineManager &pipeline_manager_;
    };

    export class ViewportRendererFactory
    {
      public:
        using Dependencies =
            TypeList<VulkanDevice, vk::SurfaceKHR, VulkanSwapchain, VulkanBufferManager, VulkanPipelineManager>;

        ViewportRendererFactory(VulkanDevice &device,
                                vk::SurfaceKHR surface,
                                const VulkanSwapchain &swapchain,
                                VulkanBufferManager &buffer_manager,
                                VulkanPipelineManager &pipeline_manager);

        std::unique_ptr<ViewportRenderer> create(const Viewport &viewport);

      private:
        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        const VulkanSwapchain &swapchain_;
        VulkanBufferManager &buffer_manager_;
        VulkanPipelineManager &pipeline_manager_;
    };
} // namespace retro
