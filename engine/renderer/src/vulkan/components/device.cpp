/**
 * @file device.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.components.device;

namespace retro
{
    VulkanDevice::VulkanDevice(const VulkanDeviceConfig &config, const vk::Device device)
        : physical_device_{config.physical_device}, graphics_family_index_{config.graphics_family},
          present_family_index_{config.present_family}, device_{device},
          graphics_queue_{device_.getQueue(graphics_family_index_, 0)},
          present_queue_{device_.getQueue(present_family_index_, 0)}
    {
    }
} // namespace retro
