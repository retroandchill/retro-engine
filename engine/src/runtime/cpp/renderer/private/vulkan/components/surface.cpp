/**
 * @file surface.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.components.surface;

import retro.core.util.exceptions;

import sdl;
import sdl.vulkan;

namespace retro
{
    vk::UniqueSurfaceKHR create_surface(const Window &viewport, vk::Instance instance)
    {
        switch (auto [backend, handle] = viewport.platform_handle(); backend)
        {
            case WindowBackend::sdl3:
                {
                    vk::SurfaceKHR::CType surface;
                    SDL::Vulkan_CreateSurface(static_cast<SDL::WindowRaw>(handle), instance, nullptr, &surface);

                    return vk::UniqueSurfaceKHR{surface, instance};
                }
        }

        throw std::invalid_argument{"Unsupported window backend"};
    }
} // namespace retro
