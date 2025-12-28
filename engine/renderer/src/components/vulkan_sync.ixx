//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:components.vulkan_sync;

import retro.core;
import std;
import vulkan_hpp;

namespace retro
{
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

        [[nodiscard]] inline size_t frame_count() const noexcept
        {
            return image_available_.size();
        }

      private:
        std::vector<vk::UniqueSemaphore> image_available_;
        std::vector<vk::UniqueSemaphore> render_finished_;
        std::vector<vk::UniqueFence> in_flight_;
    };
} // namespace retro
