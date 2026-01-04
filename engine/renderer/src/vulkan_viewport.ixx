/**
 * @file vulkan_viewport.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer:vulkan_viewport;

import retro.core;
import vulkan_hpp;

namespace retro
{
    export class VulkanViewport
    {
      public:
        virtual ~VulkanViewport() = default;

        virtual vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const = 0;

        [[nodiscard]] virtual Vector2u size() const = 0;

        [[nodiscard]] inline uint32 width() const
        {
            return size().x;
        }

        [[nodiscard]] inline uint32 height() const
        {
            return size().y;
        }
    };
} // namespace retro
