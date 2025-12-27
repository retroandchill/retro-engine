//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_sync;

import retro.core;
import std;

namespace retro
{
    export struct SyncConfig
    {
        VkDevice device = VK_NULL_HANDLE;
        uint32 frames_in_flight = 0;
    };

    export class RETRO_API VulkanSyncObjects
    {
      public:
        explicit inline VulkanSyncObjects(const SyncConfig &cfg)
        {
            create(cfg);
        }

        inline ~VulkanSyncObjects() noexcept
        {
            reset();
        }

        VulkanSyncObjects(const VulkanSyncObjects &) = delete;
        VulkanSyncObjects &operator=(const VulkanSyncObjects &) = delete;
        VulkanSyncObjects(VulkanSyncObjects &&) = delete;
        VulkanSyncObjects &operator=(VulkanSyncObjects &&) = delete;

        void recreate(const SyncConfig &cfg);
        void reset() noexcept;

        [[nodiscard]] inline VkSemaphore image_available(size_t frame) const noexcept
        {
            return image_available_[frame];
        }

        [[nodiscard]] inline VkSemaphore render_finished(size_t frame) const noexcept
        {
            return render_finished_[frame];
        }

        [[nodiscard]] inline VkFence in_flight(size_t frame) const noexcept
        {
            return in_flight_[frame];
        }

        [[nodiscard]] inline size_t frame_count() const noexcept
        {
            return image_available_.size();
        }

      private:
        void create(const SyncConfig &cfg);

        VkDevice device_{VK_NULL_HANDLE};
        std::vector<VkSemaphore> image_available_;
        std::vector<VkSemaphore> render_finished_;
        std::vector<VkFence> in_flight_;
    };
} // namespace retro
