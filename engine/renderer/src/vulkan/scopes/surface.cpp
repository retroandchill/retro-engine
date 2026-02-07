/**
 * @file surface.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.scopes.surface;

import retro.renderer.vulkan.backends.sdl;

namespace retro
{
    namespace
    {
        vk::UniqueSurfaceKHR create_surface(const Window &window, const vk::Instance instance)
        {
            switch (auto [backend, handle] = window.native_handle(); backend)
            {
                case WindowBackend::SDL3:
                    return sdl::create_surface(instance, handle);
            }

            throw std::runtime_error{"Unsupported window backend"};
        }
    } // namespace

    VulkanSurface::VulkanSurface(Window &window, const vk::Instance instance)
        : window_{window}, surface_{create_surface(window, instance)}
    {
    }
} // namespace retro
