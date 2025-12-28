//
// Created by fcors on 12/27/2025.
//
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

        [[nodiscard]] virtual Size2<uint32> size() const = 0;

        [[nodiscard]] inline uint32 width() const
        {
            return size().width;
        }

        [[nodiscard]] inline uint32 height() const
        {
            return size().height;
        }
    };
} // namespace retro