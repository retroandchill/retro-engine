/**
 * @file sync.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer:components.sync;

import vulkan_hpp;
import std;

namespace retro
{
    export struct SyncConfig
    {
        vk::Device device = nullptr;
        std::uint32_t frames_in_flight = 0;
        std::uint32_t swapchain_image_count = 0;
    };

    class VulkanSyncObjects
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
