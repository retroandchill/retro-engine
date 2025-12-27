//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>
#include <vulkan/vulkan.hpp>

module retro.renderer;

namespace retro
{
    VulkanDevice::VulkanDevice(vk::Instance instance, vk::SurfaceKHR surface) :
        physical_device_{pick_physical_device(instance, surface, graphics_family_index_, present_family_index_)},
        device_{create_device(physical_device_, graphics_family_index_, present_family_index_)},
        graphics_queue_{device_->getQueue(graphics_family_index_, 0)},
        present_queue_{device_->getQueue(present_family_index_, 0)}
    {
    }

    vk::PhysicalDevice VulkanDevice::pick_physical_device(vk::Instance instance,
                                                        vk::SurfaceKHR surface,
                                                        uint32 &out_graphics_family,
                                                        uint32 &out_present_family)
    {
        uint32 device_count = 0;
        vkEnumeratePhysicalDevices(instance, &device_count, nullptr);
        if (device_count == 0)
        {
            throw std::runtime_error{"VulkanDevice: no GPUs with Vulkan support found"};
        }

        std::vector<VkPhysicalDevice> devices(device_count);
        vkEnumeratePhysicalDevices(instance, &device_count, devices.data());

        for (VkPhysicalDevice dev : devices)
        {
            uint32 graphics_family = UINT32_MAX;
            uint32 present_family = UINT32_MAX;

            if (is_device_suitable(dev, surface, graphics_family, present_family))
            {
                out_graphics_family = graphics_family;
                out_present_family = present_family;
                return dev;
            }
        }

        throw std::runtime_error{"VulkanDevice: failed to find a suitable GPU"};
    }

    bool VulkanDevice::is_device_suitable(const vk::PhysicalDevice device,
                                          const vk::SurfaceKHR surface,
                                          uint32 &out_graphics_family,
                                          uint32 &out_present_family)
    {
        // Find queue families
        uint32 queue_family_count = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, nullptr);

        std::vector<VkQueueFamilyProperties> families(queue_family_count);
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queue_family_count, families.data());

        out_graphics_family = std::numeric_limits<uint32>::max();
        out_present_family = std::numeric_limits<uint32>::max();

        for (uint32 i = 0; i < queue_family_count; ++i)
        {
            if (families[i].queueFlags & VK_QUEUE_GRAPHICS_BIT)
            {
                out_graphics_family = i;
            }

            VkBool32 present_support = VK_FALSE;
            vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &present_support);
            if (present_support == VK_TRUE)
            {
                out_present_family = i;
            }

            if (out_graphics_family != std::numeric_limits<uint32>::max() &&
                out_present_family != std::numeric_limits<uint32>::max())
            {
                break;
            }
        }

        if (out_graphics_family == std::numeric_limits<uint32>::max() ||
            out_present_family == std::numeric_limits<uint32>::max())
        {
            return false;
        }

        // Check swapchain support (at least one format & present mode)
        uint32 format_count = 0;
        vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &format_count, nullptr);
        if (format_count == 0)
        {
            return false;
        }

        uint32 present_mode_count = 0;
        vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &present_mode_count, nullptr);
        if (present_mode_count == 0)
        {
            return false;
        }

        return true;
    }

    vk::UniqueDevice VulkanDevice::create_device(const vk::PhysicalDevice physical_device, const uint32 graphics_family, const uint32 present_family)
    {
        // Required device extensions
        constexpr std::array DEVICE_EXTENSIONS = {vk::KHRSwapchainExtensionName};

        std::set unique_families{graphics_family, present_family};

        float queue_priority = 1.0f;
        std::vector<vk::DeviceQueueCreateInfo> queue_infos;
        queue_infos.reserve(unique_families.size());

        for (uint32 family : unique_families)
        {
            queue_infos.emplace_back(vk::DeviceQueueCreateFlags{},
                family,
                1,
                &queue_priority);
        }

        vk::PhysicalDeviceFeatures device_features{}; // enable specific features as needed

        vk::DeviceCreateInfo create_info{
                {},
                static_cast<uint32>(queue_infos.size()),
                queue_infos.data(),
                0,
                nullptr,
                DEVICE_EXTENSIONS.size(),
                DEVICE_EXTENSIONS.data(),
                &device_features
            };

        return physical_device.createDeviceUnique(create_info, nullptr);
    }
} // namespace retro