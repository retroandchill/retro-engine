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
    void add_rendering_services(ServiceCollection &services, std::shared_ptr<Window> viewport, RenderBackend backend)
    {
        auto window_backend = viewport.get()->native_handle().backend;
        services.add_singleton(std::move(viewport));
        switch (backend)
        {
            case RenderBackend::Vulkan:
                add_vulkan_services(services, window_backend);
                break;
            default:
                throw std::invalid_argument("Invalid render backend");
        }
    }
} // namespace retro
