//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_swapchain;

import retro.core;
import :components.vulkan_device;
import :components.vulkan_surface;

namespace retro
{
    export struct SwapchainConfig
    {
        VkPhysicalDevice physical_device = VK_NULL_HANDLE;
        VkDevice device = VK_NULL_HANDLE;
        VkSurfaceKHR surface = VK_NULL_HANDLE;
        uint32 graphics_family = VK_QUEUE_FAMILY_IGNORED;
        uint32 present_family = VK_QUEUE_FAMILY_IGNORED;
        uint32 width = 0;
        uint32 height = 0;
    };

    export class RETRO_API VulkanSwapchain
    {
      public:
        explicit inline VulkanSwapchain(const SwapchainConfig &config)
        {
            create(config);
        }

        inline ~VulkanSwapchain() noexcept
        {
            reset();
        }

        VulkanSwapchain(const VulkanSwapchain &) = delete;
        VulkanSwapchain &operator=(const VulkanSwapchain &) = delete;
        VulkanSwapchain(VulkanSwapchain &&) = delete;
        VulkanSwapchain &operator=(VulkanSwapchain &&) = delete;

        void recreate(const SwapchainConfig &cfg);

        void reset() noexcept;

        [[nodiscard]] inline VkSwapchainKHR handle() const noexcept
        {
            return swapchain_;
        }
        [[nodiscard]] inline VkFormat format() const noexcept
        {
            return format_;
        }
        [[nodiscard]] inline VkExtent2D extent() const noexcept
        {
            return extent_;
        }
        [[nodiscard]] inline const std::vector<VkImageView> &image_views() const noexcept
        {
            return image_views_;
        }

      private:
        void create(const SwapchainConfig &cfg);

        void create_image_views();

        VkDevice device_{VK_NULL_HANDLE};
        VkSwapchainKHR swapchain_{VK_NULL_HANDLE};
        std::vector<VkImage> images_;
        std::vector<VkImageView> image_views_;
        VkFormat format_{VK_FORMAT_UNDEFINED};
        VkExtent2D extent_{};
    };
} // namespace retro
