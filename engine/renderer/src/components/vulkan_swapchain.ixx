/**
 * @file vulkan_swapchain.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:components.vulkan_swapchain;

import vulkan_hpp;
import retro.core;
import :components.vulkan_device;

namespace retro
{
    export struct SwapchainConfig
    {
        vk::PhysicalDevice physical_device = nullptr;
        vk::Device device = nullptr;
        vk::SurfaceKHR surface = nullptr;
        uint32 graphics_family = vk::QueueFamilyIgnored;
        uint32 present_family = vk::QueueFamilyIgnored;
        uint32 width = 0;
        uint32 height = 0;
        vk::SwapchainKHR old_swapchain = nullptr;
    };

    export class RETRO_API VulkanSwapchain
    {
      public:
        explicit VulkanSwapchain(const SwapchainConfig &config);

        [[nodiscard]] inline vk::SwapchainKHR handle() const noexcept
        {
            return swapchain_.get();
        }
        [[nodiscard]] inline vk::Format format() const noexcept
        {
            return format_;
        }
        [[nodiscard]] inline vk::Extent2D extent() const noexcept
        {
            return extent_;
        }
        [[nodiscard]] inline const std::vector<vk::UniqueImageView> &image_views() const noexcept
        {
            return image_views_;
        }

      private:
        void create_image_views(vk::Device device);

        vk::UniqueSwapchainKHR swapchain_{};
        std::vector<vk::Image> images_;
        std::vector<vk::UniqueImageView> image_views_;
        vk::Format format_{vk::Format::eUndefined};
        vk::Extent2D extent_{};
    };
} // namespace retro
