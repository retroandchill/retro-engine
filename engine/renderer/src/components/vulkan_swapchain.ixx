//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.hpp>

export module retro.renderer:components.vulkan_swapchain;

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
