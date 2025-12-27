//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

import retro.core;
import std;

namespace retro
{
    void VulkanSyncObjects::recreate(const SyncConfig &cfg)
    {
        reset();
        create(cfg);
    }

    void VulkanSyncObjects::reset() noexcept
    {
        if (device_ != VK_NULL_HANDLE)
        {
            for (size_t i = 0; i < image_available_.size(); ++i)
            {
                vkDestroySemaphore(device_, render_finished_[i], nullptr);
                vkDestroySemaphore(device_, image_available_[i], nullptr);
                vkDestroyFence(device_, in_flight_[i], nullptr);
            }
        }

        image_available_.clear();
        render_finished_.clear();
        in_flight_.clear();
        device_ = VK_NULL_HANDLE;
    }

    void VulkanSyncObjects::create(const SyncConfig &cfg)
    {
        if (!cfg.device || cfg.frames_in_flight == 0)
        {
            throw std::runtime_error{"VulkanSyncObjects: invalid config"};
        }

        device_ = cfg.device;

        image_available_.resize(cfg.frames_in_flight);
        render_finished_.resize(cfg.frames_in_flight);
        in_flight_.resize(cfg.frames_in_flight);

        VkSemaphoreCreateInfo sem_info{};
        sem_info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;

        VkFenceCreateInfo fence_info{};
        fence_info.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
        fence_info.flags = VK_FENCE_CREATE_SIGNALED_BIT;

        for (size_t i = 0; i < cfg.frames_in_flight; ++i)
        {
            if (vkCreateSemaphore(device_, &sem_info, nullptr, &image_available_[i]) != VK_SUCCESS ||
                vkCreateSemaphore(device_, &sem_info, nullptr, &render_finished_[i]) != VK_SUCCESS ||
                vkCreateFence(device_, &fence_info, nullptr, &in_flight_[i]) != VK_SUCCESS)
            {
                throw std::runtime_error{"VulkanSyncObjects: failed to create sync objects"};
            }
        }
    }
} // namespace retro
