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

namespace retro
{
    namespace
    {
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

        vk::PhysicalDevice pick_physical_device(vk::Instance instance,
                                                vk::SurfaceKHR surface,
                                                std::uint32_t &out_graphics_family,
                                                std::uint32_t &out_present_family)
        {
            auto devices = instance.enumeratePhysicalDevices();

            for (auto dev : devices)
            {
                std::uint32_t graphics_family = std::numeric_limits<std::uint32_t>::max();
                std::uint32_t present_family = std::numeric_limits<std::uint32_t>::max();

                if (is_device_suitable(dev, surface, graphics_family, present_family))
                {
                    out_graphics_family = graphics_family;
                    out_present_family = present_family;
                    return dev;
                }
            }

            throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_device(const vk::PhysicalDevice physical_device,
                                       const std::uint32_t graphics_family,
                                       const std::uint32_t present_family)
        {
            // Required device extensions
            constexpr std::array DEVICE_EXTENSIONS = {vk::KHRSwapchainExtensionName};

            std::set unique_families{graphics_family, present_family};

            float queue_priority = 1.0f;
            std::vector<vk::DeviceQueueCreateInfo> queue_infos;
            queue_infos.reserve(unique_families.size());

            for (std::uint32_t family : unique_families)
            {
                queue_infos.emplace_back(vk::DeviceQueueCreateInfo{.queueFamilyIndex = family,
                                                                   .queueCount = 1,
                                                                   .pQueuePriorities = &queue_priority});
            }

            vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

            vk::DeviceCreateInfo create_info{.queueCreateInfoCount = static_cast<std::uint32_t>(queue_infos.size()),
                                             .pQueueCreateInfos = queue_infos.data(),
                                             .enabledExtensionCount = DEVICE_EXTENSIONS.size(),
                                             .ppEnabledExtensionNames = DEVICE_EXTENSIONS.data(),
                                             .pEnabledFeatures = &device_features};

            return physical_device.createDeviceUnique(create_info, nullptr);
        }
    } // namespace

    VulkanDevice::VulkanDevice(const vk::Instance instance, const vk::SurfaceKHR surface)
        : physical_device_{pick_physical_device(instance, surface, graphics_family_index_, present_family_index_)},
          device_{create_device(physical_device_, graphics_family_index_, present_family_index_)},
          graphics_queue_{device_->getQueue(graphics_family_index_, 0)},
          present_queue_{device_->getQueue(present_family_index_, 0)}
    {
    }
} // namespace retro
