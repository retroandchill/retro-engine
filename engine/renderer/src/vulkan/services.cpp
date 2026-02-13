/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.services;

import vulkan_hpp;
import retro.logging;
import retro.core.type_traits.callable;
import retro.runtime.rendering.renderer2d;
import retro.platform.backend;
import retro.platform.window;
import retro.renderer.vulkan.renderer;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.command_pool;
import retro.renderer.vulkan.components.viewport_renderer;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.surface;

namespace retro
{
    namespace
    {
        VulkanSwapchain create_swapchain(const Window &window, const vk::SurfaceKHR surface, const VulkanDevice &device)
        {
            return VulkanSwapchain{SwapchainConfig{
                .physical_device = device.physical_device(),
                .device = device.device(),
                .surface = surface,
                .graphics_family = device.graphics_family_index(),
                .present_family = device.present_family_index(),
                .width = window.width(),
                .height = window.height(),
            }};
        }
    } // namespace

    void add_vulkan_services(ServiceCollection &services, WindowBackend window_backend)
    {
        services.add_singleton<Renderer2D, VulkanRenderer2D>()
            .add_singleton([window_backend] { return VulkanInstance::create(window_backend); })
            .add_singleton<&create_surface>()
            .add_singleton<&create_swapchain>()
            .add_singleton([](const VulkanInstance &instance, PlatformBackend &platform_backend)
                           { return VulkanDevice::create(instance, platform_backend); })
            .add_singleton<VulkanBufferManager>()
            .add_singleton<VulkanPipelineManager>()
            .add_singleton(
                [](const VulkanDevice &device)
                {
                    return std::make_unique<VulkanCommandPool>(
                        CommandPoolConfig{.device = device.device(),
                                          .queue_family_idx = device.graphics_family_index(),
                                          .buffer_count = VulkanRenderer2D::MAX_FRAMES_IN_FLIGHT});
                })
            .add_singleton<ViewportRendererFactory>();
    }
} // namespace retro
