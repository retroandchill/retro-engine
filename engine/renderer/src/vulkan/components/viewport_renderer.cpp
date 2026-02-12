/**
 * @file viewport_renderer.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.components.viewport_renderer;

namespace retro
{
    ViewportRenderer::ViewportRenderer(const Viewport &viewport,
                                       VulkanDevice &device,
                                       const vk::SurfaceKHR surface,
                                       const VulkanSwapchain &swapchain,
                                       VulkanBufferManager &buffer_manager,
                                       VulkanPipelineManager &pipeline_manager)
        : viewport_{viewport}, surface_{surface}, device_{device}, swapchain_{swapchain},
          buffer_manager_{buffer_manager}, pipeline_manager_{pipeline_manager}
    {
    }

    void ViewportRenderer::render_viewport(const vk::CommandBuffer cmd,
                                           const Vector2u &framebuffer_size,
                                           const vk::DescriptorPool descriptor_pool)
    {
        auto [x, y, width, height] = viewport_.screen_layout().to_screen_rect(framebuffer_size);

        // Set viewport and scissor for this viewport only
        const vk::Viewport vp{.x = static_cast<float>(x),
                              .y = static_cast<float>(y),
                              .width = static_cast<float>(width),
                              .height = static_cast<float>(height),
                              .minDepth = 0.0f,
                              .maxDepth = 1.0f};

        const vk::Rect2D scissor{.offset = vk::Offset2D{.x = x, .y = y},
                                 .extent = vk::Extent2D{.width = width, .height = height}};

        cmd.setViewport(0, vp);
        cmd.setScissor(0, scissor);

        pipeline_manager_.bind_and_render(cmd, framebuffer_size, viewport_, descriptor_pool, buffer_manager_);
    }

    ViewportRendererFactory::ViewportRendererFactory(VulkanDevice &device,
                                                     vk::SurfaceKHR surface,
                                                     const VulkanSwapchain &swapchain,
                                                     VulkanBufferManager &buffer_manager,
                                                     VulkanPipelineManager &pipeline_manager)
        : surface_{surface}, device_{device}, swapchain_{swapchain}, buffer_manager_{buffer_manager},
          pipeline_manager_{pipeline_manager}
    {
    }

    std::unique_ptr<ViewportRenderer> ViewportRendererFactory::create(const Viewport &viewport)
    {
        return std::make_unique<ViewportRenderer>(viewport,
                                                  device_,
                                                  surface_,
                                                  swapchain_,
                                                  buffer_manager_,
                                                  pipeline_manager_);
    }
} // namespace retro
