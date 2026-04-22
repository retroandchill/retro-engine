/**
 * @file services.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <stdexcept>
#endif

module retro.renderer.services;

import std;
import retro.runtime.rendering.headless_render_backend;
import retro.renderer.vulkan.vulkan_render_backend;

namespace retro
{
    RefCountPtr<RenderBackend> create_render_backend(PlatformBackend &platform_backend, const RenderBackendType backend)
    {
        switch (backend)
        {
            case RenderBackendType::headless:
                return make_ref_counted<HeadlessRenderBackend>();
            case RenderBackendType::vulkan:
                return make_ref_counted<VulkanRenderBackend>(platform_backend);
            default:
                throw std::invalid_argument("Invalid render backend");
        }
    }
} // namespace retro
