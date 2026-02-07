/**
 * @file device.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.scope.device;

import vulkan_hpp;
import retro.core.di;

namespace retro
{
    export struct VulkanDeviceCreateInfo
    {
        vk::Instance instance = nullptr;
        bool require_swapchain = true;
    };

    export class VulkanDevice
    {
        explicit VulkanDevice(vk::PhysicalDevice physical_device,
                              vk::UniqueDevice device,
                              std::uint32_t graphics_family_index);

      public:
        static VulkanDevice create(const VulkanDeviceCreateInfo &create_info);

        [[nodiscard]] inline vk::PhysicalDevice physical_device() const noexcept
        {
            return physical_device_;
        }
        [[nodiscard]] inline vk::Device device() const noexcept
        {
            return device_.get();
        }
        [[nodiscard]] inline vk::Queue graphics_queue() const noexcept
        {
            return graphics_queue_;
        }
        [[nodiscard]] inline vk::Queue present_queue() const noexcept
        {
            return present_queue_;
        }
        [[nodiscard]] inline std::uint32_t graphics_family_index() const noexcept
        {
            return graphics_family_index_;
        }
        [[nodiscard]] inline std::uint32_t present_family_index() const noexcept
        {
            return present_family_index_;
        }

      private:
        vk::PhysicalDevice physical_device_{};
        std::uint32_t graphics_family_index_{std::numeric_limits<std::uint32_t>::max()};
        std::uint32_t present_family_index_{std::numeric_limits<std::uint32_t>::max()};
        vk::UniqueDevice device_{};
        vk::Queue graphics_queue_{};
        vk::Queue present_queue_{};
    };
} // namespace retro
