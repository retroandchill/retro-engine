/**
 * @file services.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.services;

import retro.core.di;

import retro.platform.window;

namespace retro
{
    export void add_vulkan_services(ServiceCollection &services, WindowBackend window_backend);
}
