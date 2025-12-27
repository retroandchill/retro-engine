//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"
#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_device;

import retro.core;
import :components.vulkan_instance;
import std;

namespace retro
{
    export class RETRO_API VulkanDevice
    {
    public:
        VulkanDevice(const VulkanInstance& instance, const VkSurfaceKHR surface);
        ~VulkanDevice() noexcept;

        VulkanDevice(const VulkanDevice&) = delete;
        VulkanDevice& operator=(const VulkanDevice&) = delete;
        VulkanDevice(VulkanDevice&&) = delete;
        VulkanDevice& operator=(VulkanDevice&&) = delete;

        [[nodiscard]] inline VkPhysicalDevice physical_device() const noexcept { return physical_device_; }
        [[nodiscard]] inline VkDevice         device() const noexcept { return device_; }
        [[nodiscard]] inline VkQueue          graphics_queue() const noexcept { return graphics_queue_; }
        [[nodiscard]] inline VkQueue          present_queue() const noexcept { return present_queue_; }
        [[nodiscard]] inline uint32         graphics_family_index() const noexcept { return graphics_family_index_; }
        [[nodiscard]] inline uint32         present_family_index() const noexcept { return present_family_index_; }

    private:
        VkPhysicalDevice physical_device_{VK_NULL_HANDLE};
        VkDevice         device_{VK_NULL_HANDLE};
        VkQueue          graphics_queue_{VK_NULL_HANDLE};
        VkQueue          present_queue_{VK_NULL_HANDLE};
        uint32         graphics_family_index_{UINT32_MAX};
        uint32         present_family_index_{UINT32_MAX};

        static VkPhysicalDevice pick_physical_device(VkInstance instance, VkSurfaceKHR surface,
                                                     uint32& out_graphics_family,
                                                     uint32& out_present_family);

        static bool is_device_suitable(VkPhysicalDevice device, VkSurfaceKHR surface,
                                       uint32& out_graphics_family,
                                       uint32& out_present_family);
    };
}