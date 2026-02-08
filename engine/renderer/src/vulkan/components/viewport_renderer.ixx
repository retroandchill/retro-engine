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
        std::int32_t x = 0;
        std::int32_t y = 0;
        std::uint32_t width;
        std::uint32_t height;
        std::uint32_t z_order = 0;
        std::uint32_t frames_in_flight = 2;
    };

    export class ViewportRenderer
    {
      public:
        ViewportRenderer(const ViewportConfig &config,
                         VulkanDevice &device,
                         vk::SurfaceKHR surface,
                         const VulkanSwapchain &swapchain,
                         VulkanBufferManager &buffer_manager,
                         VulkanPipelineManager &pipeline_manager);

        void record_command_buffer(vk::RenderPass render_pass,
                                   vk::CommandBuffer cmd,
                                   vk::Framebuffer framebuffer,
                                   vk::DescriptorPool descriptor_pool);

      private:
        ViewportConfig config_;
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

        std::unique_ptr<ViewportRenderer> create(const ViewportConfig &config);

      private:
        vk::SurfaceKHR surface_;
        VulkanDevice &device_;
        const VulkanSwapchain &swapchain_;
        VulkanBufferManager &buffer_manager_;
        VulkanPipelineManager &pipeline_manager_;
    };
} // namespace retro
