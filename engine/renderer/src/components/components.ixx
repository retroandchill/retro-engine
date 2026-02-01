/**
 * @file components.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer:components;

import retro.core;
import vulkan_hpp;
import std;

namespace retro
{
    export struct CommandPoolConfig
    {
        vk::Device device = nullptr;
        uint32 queue_family_idx = vk::QueueFamilyIgnored;
        uint32 buffer_count = 0; // typically MAX_FRAMES_IN_FLIGHT
    };

    export class RETRO_API VulkanCommandPool
    {
      public:
        explicit VulkanCommandPool(const CommandPoolConfig &cfg);

        [[nodiscard]] inline vk::CommandPool pool() const noexcept
        {
            return pool_.get();
        }
        [[nodiscard]] inline const std::vector<vk::UniqueCommandBuffer> &buffers() const noexcept
        {
            return buffers_;
        }
        [[nodiscard]] inline vk::CommandBuffer buffer_at(const size_t index) const noexcept
        {
            return buffers_[index].get();
        }

        vk::CommandBuffer begin_single_time_commands();

        void end_single_time_commands(vk::CommandBuffer command_buffer, vk::Queue queue);

      private:
        vk::UniqueCommandPool pool_{nullptr};
        std::vector<vk::UniqueCommandBuffer> buffers_{};
    };

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

    export struct SwapchainConfig
    {
        vk::PhysicalDevice physical_device = nullptr;
        vk::Device device = nullptr;
        vk::SurfaceKHR surface = nullptr;
        uint32 graphics_family = vk::QueueFamilyIgnored;
        uint32 present_family = vk::QueueFamilyIgnored;
        uint32 width = 0;
        uint32 height = 0;
        vk::SwapchainKHR old_swapchain = nullptr;
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

    export struct SyncConfig
    {
        vk::Device device = nullptr;
        uint32 frames_in_flight = 0;
        uint32 swapchain_image_count = 0;
    };

    export class RETRO_API VulkanSyncObjects
    {
      public:
        explicit VulkanSyncObjects(const SyncConfig &cfg);

        ~VulkanSyncObjects() = default;

        VulkanSyncObjects(const VulkanSyncObjects &) = delete;
        VulkanSyncObjects(VulkanSyncObjects &&) noexcept = default;
        VulkanSyncObjects &operator=(const VulkanSyncObjects &) = delete;
        VulkanSyncObjects &operator=(VulkanSyncObjects &&) noexcept = default;

        [[nodiscard]] inline vk::Semaphore image_available(const size_t frame) const noexcept
        {
            return image_available_[frame].get();
        }

        [[nodiscard]] inline vk::Semaphore render_finished(const size_t frame) const noexcept
        {
            return render_finished_[frame].get();
        }

        [[nodiscard]] inline vk::Fence in_flight(const size_t frame) const noexcept
        {
            return in_flight_[frame].get();
        }

        [[nodiscard]] inline vk::DescriptorPool descriptor_pool(const size_t frame) const noexcept
        {
            return descriptor_pools_[frame].get();
        }

        [[nodiscard]] inline size_t frame_count() const noexcept
        {
            return image_available_.size();
        }

      private:
        std::vector<vk::UniqueSemaphore> image_available_;
        std::vector<vk::UniqueSemaphore> render_finished_;
        std::vector<vk::UniqueFence> in_flight_;
        std::vector<vk::UniqueDescriptorPool> descriptor_pools_;
    };
} // namespace retro
