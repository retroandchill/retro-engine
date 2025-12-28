//
// Created by fcors on 12/27/2025.
//
module;

#include <vulkan/vulkan.hpp>

export module retro.renderer:vulkan_viewport;

import retro.core;

namespace retro
{
    export class VulkanViewport
    {
      public:
        virtual ~VulkanViewport() = default;

        virtual vk::UniqueSurfaceKHR create_surface(vk::Instance instance) const = 0;

        virtual int32 width() const = 0;

        virtual int32 height() const = 0;
    };
} // namespace retro