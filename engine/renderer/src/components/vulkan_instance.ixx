//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_instance;

import std;

namespace retro
{
    export class VulkanInstance
    {
      public:
        inline explicit VulkanInstance(const VkInstanceCreateInfo &create_info)
        {
            if (vkCreateInstance(&create_info, nullptr, &instance_) != VK_SUCCESS)
            {
                throw std::runtime_error{"Failed to create Vulkan instance"};
            }
        }

        inline ~VulkanInstance() noexcept
        {
            vkDestroyInstance(instance_, nullptr);
        }

        VulkanInstance(const VulkanInstance &) = delete;
        VulkanInstance(VulkanInstance &&) = delete;
        VulkanInstance &operator=(const VulkanInstance &) = delete;
        VulkanInstance &operator=(VulkanInstance &&) = delete;

        [[nodiscard]] inline VkInstance instance() const noexcept
        {
            return instance_;
        }

      private:
        VkInstance instance_{VK_NULL_HANDLE};
    };
} // namespace retro