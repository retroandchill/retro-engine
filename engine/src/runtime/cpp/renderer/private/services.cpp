/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.services;

import retro.renderer.vulkan.services;

namespace retro
{
    void add_rendering_services(ServiceCollection &services,
                                const WindowBackend window_backend,
                                const RenderBackend backend)
    {
        switch (backend)
        {
            case RenderBackend::vulkan:
                add_vulkan_services(services, window_backend);
                break;
            default:
                throw std::invalid_argument("Invalid render backend");
        }
    }
} // namespace retro
