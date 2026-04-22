/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.services;

import vulkan;
import retro.logging;
import retro.core.type_traits.callable;
import retro.runtime.rendering.renderer2d;
import retro.platform.backend;
import retro.platform.window;
import retro.renderer.vulkan.renderer;
import retro.runtime.rendering.texture_manager;
import retro.renderer.vulkan.texture_manager;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.presenter;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.surface;

namespace retro
{
    void add_vulkan_services(ServiceCollection &services, WindowBackend window_backend)
    {
        // Do nothing for now
    }
} // namespace retro
