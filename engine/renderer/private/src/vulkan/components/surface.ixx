/**
 * @file surface.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.surface;

import vulkan_hpp;
import retro.platform.window;
import retro.renderer.vulkan.components.instance;

namespace retro
{
    export vk::UniqueSurfaceKHR create_surface(const Window &viewport, const VulkanInstance &instance);
}
