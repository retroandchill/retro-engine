/**
 * @file vulkan_render_backend.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.vulkan_render_backend;
import retro.renderer.vulkan.renderer;

namespace retro
{

    VulkanRenderBackend::VulkanRenderBackend(PlatformBackend &platform_backend)
        : instance_{platform_backend.window_backend()}, device_{instance_, platform_backend},
          buffer_manager_{device_.create_buffer_manager()}, command_pool_{device_.create_command_pool()}
    {
    }

    std::unique_ptr<Renderer2D> VulkanRenderBackend::create_renderer(RefCountPtr<Window> window)
    {
        auto surface = instance_.create_surface(*window);
        return std::make_unique<VulkanRenderer2D>(*this,
                                                  std::move(window),
                                                  std::move(surface),
                                                  device_,
                                                  buffer_manager_,
                                                  command_pool_.get());
    }
} // namespace retro
