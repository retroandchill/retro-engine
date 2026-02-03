/**
 * @file swapchain.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.swapchain;

import vulkan_hpp;

namespace retro
{
    export struct SwapchainConfig
    {
        vk::PhysicalDevice physical_device = nullptr;
        vk::Device device = nullptr;
        vk::SurfaceKHR surface = nullptr;
        std::uint32_t graphics_family = vk::QueueFamilyIgnored;
        std::uint32_t present_family = vk::QueueFamilyIgnored;
        std::uint32_t width = 0;
        std::uint32_t height = 0;
        vk::SwapchainKHR old_swapchain = nullptr;
    };

    export class VulkanSwapchain
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
