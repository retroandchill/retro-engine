/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.services;

import retro.runtime.rendering.renderer2d;
import retro.renderer.vulkan.renderer;

namespace retro
{
    void add_vulkan_services(ServiceCollection &services)
    {
        services.add_singleton<Renderer2D, VulkanRenderer2D>();
    }
} // namespace retro
