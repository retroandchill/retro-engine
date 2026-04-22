/**
 * @file vulkan_render_backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.vulkan_render_backend;

import vulkan;
import retro.runtime.rendering.render_backend;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.core.memory.ref_counted_ptr;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.platform.backend;

namespace retro
{
    export class VulkanRenderBackend final : public RenderBackend
    {
      public:
        explicit VulkanRenderBackend(PlatformBackend &platform_backend);

        std::unique_ptr<Renderer2D> create_renderer(RefCountPtr<Window> window) override;

      private:
        VulkanInstance instance_;
        VulkanDevice device_;
        VulkanBufferManager buffer_manager_;
        vk::UniqueCommandPool command_pool_;
    };
} // namespace retro
