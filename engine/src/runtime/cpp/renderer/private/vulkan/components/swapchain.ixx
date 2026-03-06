/**
 * @file swapchain.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.swapchain;

import vulkan_hpp;
import retro.platform.window;
import retro.renderer.vulkan.components.device;

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

    export struct VulkanImageResources
    {
        vk::Image color_image;
        vk::UniqueImageView color_image_view;
        vk::UniqueImage depth_image;
        vk::UniqueDeviceMemory depth_image_memory;
        vk::UniqueImageView depth_image_view;
        vk::UniqueSemaphore render_finished;
        vk::UniqueFramebuffer framebuffer;
    };

    export class VulkanSwapchain
    {
      public:
        explicit VulkanSwapchain(vk::SurfaceKHR surface,
                                 VulkanDevice &device,
                                 std::uint32_t width,
                                 std::uint32_t height);

        [[nodiscard]] inline vk::SwapchainKHR handle() const noexcept
        {
            return swapchain_.get();
        }

        [[nodiscard]] inline vk::RenderPass render_pass() const noexcept
        {
            return render_pass_.get();
        }

        [[nodiscard]] inline vk::Format format() const noexcept
        {
            return format_;
        }
        [[nodiscard]] inline vk::Extent2D extent() const noexcept
        {
            return extent_;
        }

        [[nodiscard]] inline std::span<const VulkanImageResources> image_resources() const noexcept
        {
            return image_resources_;
        }

        void recreate(std::uint32_t width, std::uint32_t height);

      private:
        vk::SurfaceKHR surface_;
        VulkanDevice &device_;

        vk::UniqueSwapchainKHR swapchain_{};
        vk::UniqueRenderPass render_pass_;
        std::vector<VulkanImageResources> image_resources_;
        vk::Format format_{vk::Format::eUndefined};
        vk::Extent2D extent_{};
    };
} // namespace retro
