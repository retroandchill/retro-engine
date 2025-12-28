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
    VulkanSyncObjects::VulkanSyncObjects(const SyncConfig &cfg)
    {
        if (!cfg.device || cfg.frames_in_flight == 0)
        {
            throw std::runtime_error{"VulkanSyncObjects: invalid config"};
        };

        image_available_.reserve(cfg.frames_in_flight);
        render_finished_.reserve(cfg.frames_in_flight);
        in_flight_.reserve(cfg.frames_in_flight);

        constexpr vk::SemaphoreCreateInfo sem_info{};

        constexpr vk::FenceCreateInfo fence_info{vk::FenceCreateFlagBits::eSignaled};

        for (size_t i = 0; i < cfg.frames_in_flight; ++i)
        {
            image_available_.emplace_back(cfg.device.createSemaphoreUnique(sem_info));
            render_finished_.emplace_back(cfg.device.createSemaphoreUnique(sem_info));
            in_flight_.emplace_back(cfg.device.createFenceUnique(fence_info));
        }
    }
} // namespace retro
