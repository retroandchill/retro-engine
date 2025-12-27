//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    VulkanDevice::VulkanDevice(const VulkanInstance &instance, const VkSurfaceKHR surface)
    {
        if (instance.instance() == VK_NULL_HANDLE || surface == VK_NULL_HANDLE)
        {
            throw std::runtime_error{"VulkanDevice: instance or surface is null"};
        }

        physical_device_ =
            pick_physical_device(instance.instance(), surface, graphics_family_index_, present_family_index_);

        // Required device extensions
        constexpr const char *DEVICE_EXTENSIONS[] = {VK_KHR_SWAPCHAIN_EXTENSION_NAME};

        std::set unique_families{graphics_family_index_, present_family_index_};

        float queue_priority = 1.0f;
        std::vector<VkDeviceQueueCreateInfo> queue_infos;
        queue_infos.reserve(unique_families.size());

        for (uint32 family : unique_families)
        {
            VkDeviceQueueCreateInfo qi{};
            qi.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
            qi.queueFamilyIndex = family;
            qi.queueCount = 1;
            qi.pQueuePriorities = &queue_priority;
            queue_infos.push_back(qi);
        }

        VkPhysicalDeviceFeatures device_features{}; // enable specific features as needed

        VkDeviceCreateInfo create_info{};
        create_info.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
        create_info.queueCreateInfoCount = static_cast<uint32>(queue_infos.size());
        create_info.pQueueCreateInfos = queue_infos.data();
        create_info.pEnabledFeatures = &device_features;
        create_info.enabledExtensionCount = static_cast<uint32>(std::size(DEVICE_EXTENSIONS));
        create_info.ppEnabledExtensionNames = DEVICE_EXTENSIONS;

        if (vkCreateDevice(physical_device_, &create_info, nullptr, &device_) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanDevice: failed to create logical device"};
        }

        vkGetDeviceQueue(device_, graphics_family_index_, 0, &graphics_queue_);
        vkGetDeviceQueue(device_, present_family_index_, 0, &present_queue_);
    }

    VulkanDevice::~VulkanDevice() noexcept
    {
        vkDestroyDevice(device_, nullptr);
    }

    VkPhysicalDevice VulkanDevice::pick_physical_device(const VkInstance instance,
                                                        const VkSurfaceKHR surface,
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

    bool VulkanDevice::is_device_suitable(const VkPhysicalDevice device,
                                          const VkSurfaceKHR surface,
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
} // namespace retro