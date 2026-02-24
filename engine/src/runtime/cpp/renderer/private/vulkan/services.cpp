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
import retro.runtime.rendering.texture_manager;
import retro.renderer.vulkan.texture_manager;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.viewport_renderer;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.surface;
import retro.renderer.vulkan.components.sync;

namespace retro
{
    void add_vulkan_services(ServiceCollection &services, WindowBackend window_backend)
    {
        services.add_scoped<Renderer2D, VulkanRenderer2D>()
            .add_singleton<TextureManager, VulkanTextureManager>()
            .add_singleton([window_backend] { return VulkanInstance::create(window_backend); })
            .add_scoped<&create_surface>()
            .add_scoped(
                [](const Window &window, const vk::SurfaceKHR surface, const VulkanDevice &device)
                {
                    return std::make_unique<VulkanSwapchain>(SwapchainConfig{
                        .physical_device = device.physical_device(),
                        .device = device.device(),
                        .surface = surface,
                        .graphics_family = device.graphics_family_index(),
                        .present_family = device.present_family_index(),
                        .width = window.width(),
                        .height = window.height(),
                    });
                })
            .add_singleton([](const VulkanInstance &instance, PlatformBackend &platform_backend)
                           { return VulkanDevice::create(instance, platform_backend); })
            .add_singleton<VulkanBufferManager>()
            .add_scoped<VulkanPipelineManager>()
            .add_singleton(
                [](const VulkanDevice &device)
                {
                    const vk::CommandPoolCreateInfo pool_info{.flags =
                                                                  vk::CommandPoolCreateFlagBits::eResetCommandBuffer,
                                                              .queueFamilyIndex = device.graphics_family_index()};

                    return device.device().createCommandPoolUnique(pool_info);
                })
            .add_scoped<ViewportRendererFactory>();
    }
} // namespace retro
