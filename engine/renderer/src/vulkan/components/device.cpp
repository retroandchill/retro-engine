/**
 * @file device.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.components.device;

import retro.core.containers.optional;
import retro.platform.window;
import retro.renderer.vulkan.components.surface;

namespace retro
{
    namespace
    {
        Optional<std::pair<std::uint32_t, std::uint32_t>> find_graphics_and_present_families(
            const vk::PhysicalDevice device,
            const vk::SurfaceKHR surface)
        {
            auto families = device.getQueueFamilyProperties();

            std::uint32_t out_graphics_family = vk::QueueFamilyIgnored;
            std::uint32_t out_present_family = vk::QueueFamilyIgnored;

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

                if (out_graphics_family != vk::QueueFamilyIgnored && out_present_family != vk::QueueFamilyIgnored)
                {
                    break;
                }
            }

            if (out_graphics_family == std::numeric_limits<std::uint32_t>::max() ||
                out_present_family == std::numeric_limits<std::uint32_t>::max())
            {
                return std::nullopt;
            }

            // Check swapchain support (at least one format & present mode)
            std::uint32_t format_count = 0;
            auto res = device.getSurfaceFormatsKHR(surface, &format_count, nullptr);
            if (res != vk::Result::eSuccess || format_count == 0)
            {
                return std::nullopt;
            }

            std::uint32_t present_mode_count = 0;
            res = device.getSurfacePresentModesKHR(surface, &present_mode_count, nullptr);
            if (res != vk::Result::eSuccess || present_mode_count == 0)
            {
                return std::nullopt;
            }

            return std::make_pair(out_graphics_family, out_present_family);
        }

        VulkanDeviceConfig pick_physical_device(const VulkanInstance &instance, const vk::SurfaceKHR surface)
        {
            for (const auto devices = instance.handle().enumeratePhysicalDevices(); const auto dev : devices)
            {
                auto result = find_graphics_and_present_families(dev, surface);
                if (!result.has_value())
                {
                    continue;
                }

                auto [graphics_family, present_family] = *result;
                return VulkanDeviceConfig{dev, graphics_family, present_family};
            }

            throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_device(const VulkanDeviceConfig &config)
        {
            // Required device extensions
            constexpr std::array device_extensions = {vk::KHRSwapchainExtensionName};

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
                                                   .enabledExtensionCount = device_extensions.size(),
                                                   .ppEnabledExtensionNames = device_extensions.data(),
                                                   .pEnabledFeatures = &device_features};

            return config.physical_device.createDeviceUnique(create_info, nullptr);
        }
    } // namespace

    VulkanDevice::VulkanDevice(const VulkanDeviceConfig &config, vk::UniqueDevice device)
        : physical_device_{config.physical_device}, device_{std::move(device)},
          graphics_family_index_{config.graphics_family}, present_family_index_{config.present_family},
          graphics_queue_{device_->getQueue(graphics_family_index_, 0)},
          present_queue_{device_->getQueue(present_family_index_, 0)}
    {
    }

    std::unique_ptr<VulkanDevice> VulkanDevice::create(const VulkanInstance &instance,
                                                       PlatformBackend &platform_backend)
    {
        // Create a hidden window and surface to test against for the device so we can properly separate device
        // selection from surface creation.
        const auto window =
            platform_backend.create_window(WindowDesc{.flags = WindowFlags::vulkan | WindowFlags::hidden});
        auto surface = create_surface(*window, instance);
        auto config = pick_physical_device(instance, surface.get());
        return std::make_unique<VulkanDevice>(config, create_device(config));
    }
} // namespace retro
