/**
 * @file device.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.device;

import vulkan_hpp;
import retro.core.di;
import retro.renderer.vulkan.components.instance;
import retro.platform.backend;

namespace retro
{
    export struct VulkanDeviceConfig
    {
        vk::PhysicalDevice physical_device = nullptr;
        std::uint32_t graphics_family = vk::QueueFamilyIgnored;
        std::uint32_t present_family = vk::QueueFamilyIgnored;
    };

    export class VulkanDevice
    {
      public:
        VulkanDevice(const VulkanDeviceConfig &config, vk::UniqueDevice device);

        static std::unique_ptr<VulkanDevice> create(const VulkanInstance &instance, PlatformBackend &platform_backend);

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
        vk::UniqueDevice device_{};
        std::uint32_t graphics_family_index_{std::numeric_limits<std::uint32_t>::max()};
        std::uint32_t present_family_index_{std::numeric_limits<std::uint32_t>::max()};
        vk::Queue graphics_queue_{};
        vk::Queue present_queue_{};
    };
} // namespace retro
