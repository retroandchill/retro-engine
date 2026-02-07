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

module retro.renderer.vulkan.scope.device;

import retro.core.containers.optional;

namespace retro
{
    namespace
    {
        bool has_required_device_extensions(vk::PhysicalDevice physical_device, bool require_swapchain)
        {
            if (!require_swapchain)
            {
                return true;
            }

            constexpr std::array required_exts = {vk::KHRSwapchainExtensionName};
            const auto available = physical_device.enumerateDeviceExtensionProperties();

            const auto has_ext = [&](const char *name)
            {
                return std::ranges::any_of(available,
                                           [&](const vk::ExtensionProperties &ep)
                                           { return std::string_view{ep.extensionName} == name; });
            };

            for (const char *ext : required_exts)
            {
                if (!has_ext(ext))
                {
                    return false;
                }
            }

            return true;
        }

        Optional<std::uint32_t> pick_graphics_queue_family(vk::PhysicalDevice physical_device)
        {
            for (const auto [i, family] : physical_device.getQueueFamilyProperties() | std::views::enumerate)
            {
                if (family.queueFlags & vk::QueueFlagBits::eGraphics)
                {
                    return i;
                }
            }

            return std::nullopt;
        }

        std::pair<vk::PhysicalDevice, std::uint32_t> pick_physical_device(vk::Instance instance, bool require_swapchain)
        {
            for (const auto devices = instance.enumeratePhysicalDevices(); const auto dev : devices)
            {
                auto graphics_queue_family = pick_graphics_queue_family(dev);
                if (!graphics_queue_family.has_value())
                {
                    continue;
                }

                if (!has_required_device_extensions(dev, require_swapchain))
                {
                    continue;
                }

                return std::make_pair(dev, *graphics_queue_family);
            }

            throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
        }

        vk::UniqueDevice create_logical_device(vk::PhysicalDevice physical_device,
                                               std::uint32_t graphics_family_index,
                                               bool require_swapchain)
        {
            std::vector<const char *> device_exts;
            if (require_swapchain)
            {
                device_exts.push_back(vk::KHRSwapchainExtensionName);
            }

            constexpr float queue_priority = 1.0f;
            const vk::DeviceQueueCreateInfo queue_info{
                .queueFamilyIndex = graphics_family_index,
                .queueCount = 1,
                .pQueuePriorities = &queue_priority,
            };

            vk::PhysicalDeviceFeatures device_features{};

            const vk::DeviceCreateInfo create_info{
                .queueCreateInfoCount = 1,
                .pQueueCreateInfos = &queue_info,
                .enabledExtensionCount = static_cast<std::uint32_t>(device_exts.size()),
                .ppEnabledExtensionNames = device_exts.data(),
                .pEnabledFeatures = &device_features,
            };

            return physical_device.createDeviceUnique(create_info, nullptr);
        }
    } // namespace

    VulkanDevice::VulkanDevice(const vk::PhysicalDevice physical_device,
                               vk::UniqueDevice device,
                               const std::uint32_t graphics_family_index)
        : physical_device_(physical_device), graphics_family_index_(graphics_family_index),
          present_family_index_{graphics_family_index_}, device_(std::move(device)),
          graphics_queue_{device_.get().getQueue(graphics_family_index_, 0)},
          present_queue_{device_.get().getQueue(present_family_index_, 0)}

    {
    }

    VulkanDevice VulkanDevice::create(const VulkanDeviceCreateInfo &create_info)
    {
        if (create_info.instance == nullptr)
        {
            throw std::runtime_error{"VulkanDevice: instance is null"};
        }

        auto [physical_device, graphics_family] =
            pick_physical_device(create_info.instance, create_info.require_swapchain);

        auto device = create_logical_device(physical_device, graphics_family, create_info.require_swapchain);

        return VulkanDevice{physical_device, std::move(device), graphics_family};
    }
} // namespace retro
