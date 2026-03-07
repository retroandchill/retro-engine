/**
 * @file surface.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <SDL3/SDL_vulkan.h>

module retro.renderer.vulkan.components.surface;

import retro.runtime.exceptions;

namespace retro
{
    vk::UniqueSurfaceKHR create_surface(const Window &viewport, vk::Instance instance)
    {
        switch (auto [backend, handle] = viewport.native_handle(); backend)
        {
            case WindowBackend::sdl3:
                {
                    vk::SurfaceKHR::CType surface;
                    if (!SDL_Vulkan_CreateSurface(static_cast<SDL_Window *>(handle), instance, nullptr, &surface))
                    {
                        throw GraphicsException{SDL_GetError()};
                    }

                    return vk::UniqueSurfaceKHR{surface, instance};
                }
        }

        throw std::invalid_argument{"Unsupported window backend"};
    }
} // namespace retro
