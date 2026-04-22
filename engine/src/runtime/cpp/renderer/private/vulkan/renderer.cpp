/**
 * @file renderer.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.renderer;

import retro.logging;
import vulkan;
import retro.core.util.deferred;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(VulkanRenderBackend &backend,
                                       RefCountPtr<Window> window,
                                       vk::UniqueSurfaceKHR surface,
                                       VulkanDevice &device,
                                       VulkanBufferManager &buffer_manager,
                                       const vk::CommandPool command_pool)
        : backend_{backend.shared_from_this()}, window_{std::move(window)}, surface_{std::move(surface)},
          device_{device}, buffer_manager_{buffer_manager}, command_pool_{command_pool},
          pipeline_manager_{device_, buffer_manager_},
          presenter_{*window, surface.get(), device_, command_pool, pipeline_manager_}
    {
    }

    VulkanRenderer2D::~VulkanRenderer2D()
    {
        // We need to ensure that there are no in-flight frames before any members are destroyed
        device_.wait_idle();
    }

    void VulkanRenderer2D::request_stop()
    {
        renderer_teardown_source_.request_stop();
    }

    void VulkanRenderer2D::wait_for_current_frame()
    {
        presenter_.wait_for_current_frame();
    }

    void VulkanRenderer2D::queue_frame_for_render(RenderQueueFn factory)
    {
        presenter_.queue_frame_for_render(factory, renderer_teardown_source_.get_token());
    }
    void VulkanRenderer2D::render_next_available_frame()
    {

        presenter_.begin_frame();

        Deferred reset_frame_data{[this]
                                  {
                                      buffer_manager_.reset();
                                  }};

        presenter_.submit_and_present(renderer_teardown_source_.get_token());
    }

    Window &VulkanRenderer2D::window() const
    {
        return *window_;
    }

    void VulkanRenderer2D::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {
        presenter_.add_new_render_pipeline(type, pipeline);
    }

    void VulkanRenderer2D::remove_render_pipeline(const std::type_index type)
    {
        presenter_.remove_render_pipeline(type);
    }

} // namespace retro
