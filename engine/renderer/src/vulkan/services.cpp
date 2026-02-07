/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

#include <SDL3/SDL_vulkan.h>

module retro.renderer.vulkan.services;

import vulkan_hpp;
import retro.logging;
import retro.core.type_traits.callable;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.renderer.vulkan.renderer;
import retro.renderer.vulkan.scope.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.command_pool;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.scopes.instance;

namespace retro
{
    namespace
    {
        vk::UniqueSurfaceKHR create_surface(const Window &viewport, const VulkanInstance &instance)
        {
            switch (auto [backend, handle] = viewport.native_handle(); backend)
            {
                case WindowBackend::SDL3:
                    {
                        vk::SurfaceKHR::CType surface;
                        if (!SDL_Vulkan_CreateSurface(static_cast<SDL_Window *>(handle),
                                                      instance.handle(),
                                                      nullptr,
                                                      &surface))
                        {
                            throw std::runtime_error{"VulkanSurface: SDL_Vulkan_CreateSurface failed"};
                        }

                        return vk::UniqueSurfaceKHR{surface, instance.handle()};
                    }
            }

            throw std::runtime_error{"Unsupported window backend"};
        }
    } // namespace

    void add_vulkan_services(ServiceCollection &services, WindowBackend window_backend)
    {
        services.add_singleton<Renderer2D, VulkanRenderer2D>()
            .add_singleton([window_backend] { return VulkanInstance::create(window_backend); })
            .add_singleton<&create_surface>()
            .add_singleton([](const VulkanInstance &instance)
                           { return VulkanDevice::create(VulkanDeviceCreateInfo{.instance = instance.handle()}); })
            .add_singleton<VulkanBufferManager>()
            .add_singleton<VulkanPipelineManager>()
            .add_singleton(
                [](const VulkanDevice &device)
                {
                    return std::make_shared<VulkanCommandPool>(
                        CommandPoolConfig{.device = device.device(),
                                          .queue_family_idx = device.graphics_family_index(),
                                          .buffer_count = VulkanRenderer2D::MAX_FRAMES_IN_FLIGHT});
                });
    }
} // namespace retro
