//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:components.vulkan_device;

import vulkan_hpp;
import retro.core;
import std;

namespace retro
{
    export class RETRO_API VulkanDevice
    {
      public:
        VulkanDevice(vk::Instance instance, vk::SurfaceKHR surface);

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
        [[nodiscard]] inline uint32 graphics_family_index() const noexcept
        {
            return graphics_family_index_;
        }
        [[nodiscard]] inline uint32 present_family_index() const noexcept
        {
            return present_family_index_;
        }

      private:
        uint32 graphics_family_index_{std::numeric_limits<uint32>::max()};
        uint32 present_family_index_{std::numeric_limits<uint32>::max()};
        vk::PhysicalDevice physical_device_{};
        vk::UniqueDevice device_{};
        vk::Queue graphics_queue_{};
        vk::Queue present_queue_{};

        static vk::PhysicalDevice pick_physical_device(vk::Instance instance,
                                                       vk::SurfaceKHR surface,
                                                       uint32 &out_graphics_family,
                                                       uint32 &out_present_family);

        static bool is_device_suitable(vk::PhysicalDevice device,
                                       vk::SurfaceKHR surface,
                                       uint32 &out_graphics_family,
                                       uint32 &out_present_family);

        static vk::UniqueDevice create_device(vk::PhysicalDevice physical_device,
                                              uint32 graphics_family,
                                              uint32 present_family);
    };
} // namespace retro