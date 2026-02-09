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
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.command_pool;
import retro.renderer.vulkan.components.viewport_renderer;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.instance;

namespace retro
{
    namespace
    {
        vk::UniqueSurfaceKHR create_surface(const Window &viewport, const VulkanInstance &instance)
        {
            switch (auto [backend, handle] = viewport.native_handle(); backend)
            {
                case WindowBackend::sdl3:
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

        bool is_device_suitable(const vk::PhysicalDevice device,
                                const vk::SurfaceKHR surface,
                                std::uint32_t &out_graphics_family,
                                std::uint32_t &out_present_family)
        {
            auto families = device.getQueueFamilyProperties();

            out_graphics_family = std::numeric_limits<std::uint32_t>::max();
            out_present_family = std::numeric_limits<std::uint32_t>::max();

            for (std::uint32_t i = 0; i < families.size(); ++i)
            {
                if (families[i].queueFlags & vk::QueueFlagBits::eGraphics)
                {
                    out_graphics_family = i;
                }

                vk::Bool32 present_support = vk::False;
                auto res = device.getSurfaceSupportKHR(i, surface, &present_support);
                if (res == vk::Result::eSuccess && present_support == vk::True)
                {
                    out_present_family = i;
                }

                if (out_graphics_family != std::numeric_limits<std::uint32_t>::max() &&
                    out_present_family != std::numeric_limits<std::uint32_t>::max())
                {
                    break;
                }
            }

            if (out_graphics_family == std::numeric_limits<std::uint32_t>::max() ||
                out_present_family == std::numeric_limits<std::uint32_t>::max())
            {
                return false;
            }

            // Check swapchain support (at least one format & present mode)
            std::uint32_t format_count = 0;
            auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
            if (res != vk::Result::eSuccess || format_count == 0)
            {
                return false;
            }

            std::uint32_t present_mode_count = 0;
            res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
            if (res != vk::Result::eSuccess || present_mode_count == 0)
            {
                return false;
            }

            return true;
        }

        VulkanDeviceConfig pick_physical_device(const VulkanInstance &instance, const vk::SurfaceKHR surface)
        {
            for (const auto devices = instance.handle().enumeratePhysicalDevices(); const auto dev : devices)
            {
                if (VulkanDeviceConfig result{.physical_device = dev};
                    is_device_suitable(dev, surface, result.graphics_family, result.present_family))
                {
                    return result;
                }
            }

            throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_device(const VulkanDeviceConfig &config)
        {
            // Required device extensions
            constexpr std::array DEVICE_EXTENSIONS = {vk::KHRSwapchainExtensionName};

            const std::set unique_families{config.graphics_family, config.present_family};

            constexpr float queue_priority = 1.0f;
            std::vector<vk::DeviceQueueCreateInfo> queue_infos;
            queue_infos.reserve(unique_families.size());

            for (const std::uint32_t family : unique_families)
            {
                queue_infos.emplace_back(vk::DeviceQueueCreateInfo{.queueFamilyIndex = family,
                                                                   .queueCount = 1,
                                                                   .pQueuePriorities = &queue_priority});
            }

            vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

            const vk::DeviceCreateInfo create_info{.queueCreateInfoCount =
                                                       static_cast<std::uint32_t>(queue_infos.size()),
                                                   .pQueueCreateInfos = queue_infos.data(),
                                                   .enabledExtensionCount = DEVICE_EXTENSIONS.size(),
                                                   .ppEnabledExtensionNames = DEVICE_EXTENSIONS.data(),
                                                   .pEnabledFeatures = &device_features};

            return config.physical_device.createDeviceUnique(create_info, nullptr);
        }

        VulkanSwapchain create_swapchain(const Window &window,
                                         const vk::SurfaceKHR surface,
                                         const VulkanDeviceConfig &config,
                                         const vk::Device device)
        {
            return VulkanSwapchain{SwapchainConfig{
                .physical_device = config.physical_device,
                .device = device,
                .surface = surface,
                .graphics_family = config.graphics_family,
                .present_family = config.present_family,
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
            .add_singleton<&pick_physical_device>()
            .add_singleton<&create_device>()
            .add_singleton<&create_swapchain>()
            .add_singleton<VulkanDevice>()
            .add_singleton<VulkanBufferManager>()
            .add_singleton<VulkanPipelineManager>()
            .add_singleton(
                [](const VulkanDevice &device)
                {
                    return std::make_shared<VulkanCommandPool>(
                        CommandPoolConfig{.device = device.device(),
                                          .queue_family_idx = device.graphics_family_index(),
                                          .buffer_count = VulkanRenderer2D::MAX_FRAMES_IN_FLIGHT});
                })
            .add_singleton<ViewportRendererFactory>();
    }
} // namespace retro
