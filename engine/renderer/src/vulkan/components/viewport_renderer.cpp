/**
 * @file viewport_renderer.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.components.viewport_renderer;

import retro.core.math.vector;

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

    void ViewportRenderer::record_command_buffer(const vk::RenderPass render_pass,
                                                 const vk::CommandBuffer cmd,
                                                 const vk::Framebuffer framebuffer,
                                                 const vk::DescriptorPool descriptor_pool)
    {
        // ReSharper disable once CppDFAUnusedValue
        // ReSharper disable once CppDFAUnreadVariable
        vk::ClearValue color_clear_value{.color = vk::ClearColorValue{.float32 = std::array{0.0f, 0.0f, 0.0f, 1.0f}}};
        vk::ClearValue depth_clear_value{.depthStencil = vk::ClearDepthStencilValue{.depth = 1.0f}};

        std::array clear_values = {color_clear_value, depth_clear_value};

        auto [screen_width, screen_height] = swapchain_.extent();
        auto [x, y, width, height] = viewport_.screen_layout().to_screen_rect(Vector2u{screen_width, screen_height});

        const vk::RenderPassBeginInfo rp_info{.renderPass = render_pass,
                                              .framebuffer = framebuffer,
                                              .renderArea =
                                                  vk::Rect2D{.offset = vk::Offset2D{.x = x, .y = y},
                                                             .extent = vk::Extent2D{.width = width, .height = height}},
                                              .clearValueCount = clear_values.size(),
                                              .pClearValues = clear_values.data()};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);
        pipeline_manager_.bind_and_render(cmd, Vector2u{width, height}, viewport_, descriptor_pool, buffer_manager_);
        cmd.endRenderPass();
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
