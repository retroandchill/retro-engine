/**
 * @file vulkan_sync.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

import retro.core;
import std;
import vulkan_hpp;

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
            in_flight_.emplace_back(cfg.device.createFenceUnique(fence_info));
        }

        for (size_t i = 0; i < cfg.swapchain_image_count; ++i)
        {
            render_finished_.emplace_back(cfg.device.createSemaphoreUnique(sem_info));
        }
    }
} // namespace retro
