/**
 * @file device_handles_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.data.device_handles_data;

import vulkan_hpp;

namespace retro
{
    export struct VulkanDeviceHandles
    {
        vk::PhysicalDevice physical_device{};
        std::uint32_t graphics_family_index{vk::QueueFamilyIgnored};
        std::uint32_t present_family_index{vk::QueueFamilyIgnored};
        vk::Device device{};
        vk::Queue graphics_queue{};
        vk::Queue present_queue{};
    };
} // namespace retro
