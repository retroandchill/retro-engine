/**
 * @file sdl.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.backends.sdl;

import vulkan_hpp;

namespace retro::sdl
{
    export std::span<const char *const> get_required_instance_extensions();

    export vk::UniqueSurfaceKHR create_surface(vk::Instance instance, void *window);
} // namespace retro::sdl
