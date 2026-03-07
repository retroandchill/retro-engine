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
import vulkan_hpp;
import retro.core.util.deferred;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(Window &window,
                                       const vk::SurfaceKHR surface,
                                       VulkanDevice &device,
                                       VulkanPresenter &presenter,
                                       VulkanBufferManager &buffer_manager,
                                       const vk::CommandPool command_pool,
                                       VulkanPipelineManager &pipeline_manager,
                                       ViewportRendererFactory &viewport_factory)
        : window_{window}, surface_{surface}, device_{device}, buffer_manager_{buffer_manager}, presenter_(presenter),
          command_pool_(command_pool), pipeline_manager_{pipeline_manager}, viewport_factory_{viewport_factory}
    {
    }

    VulkanRenderer2D::~VulkanRenderer2D()
    {
        // We need to ensure that there are no in-flight frames before any members are destroyed
        device_.wait_idle();
    }

    void VulkanRenderer2D::wait_for_current_frame()
    {
        presenter_.wait_for_current_frame();
    }

    void VulkanRenderer2D::queue_frame_for_render(RenderQueueFn factory)
    {
        presenter_.queue_frame_for_render(factory);
    }

    void VulkanRenderer2D::begin_frame()
    {
        presenter_.begin_frame();
    }

    void VulkanRenderer2D::end_frame()
    {
        Deferred reset_frame_data{[this]
                                  {
                                      pipeline_manager_.clear_draw_queue();
                                      buffer_manager_.reset();
                                  }};

        presenter_.submit_and_present();
    }

    Window &VulkanRenderer2D::window() const
    {
        return window_;
    }

    void VulkanRenderer2D::add_new_render_pipeline(const std::type_index type, RenderPipeline &pipeline)
    {
        presenter_.add_new_render_pipeline(type, pipeline);
    }

    void VulkanRenderer2D::remove_render_pipeline(const std::type_index type)
    {
        presenter_.remove_render_pipeline(type);
    }

    void VulkanRenderer2D::add_viewport(Viewport &viewport)
    {
        viewports_.emplace_back(viewport_factory_.create(viewport));
        viewport.on_z_order_changed().add([this](Viewport &, std::int32_t) { viewports_sorted_ = false; });
        viewports_sorted_ = false;
    }

    void VulkanRenderer2D::remove_viewport(Viewport &viewport)
    {
        std::erase_if(viewports_,
                      [&](const std::unique_ptr<ViewportRenderer> &v)
                      { return std::addressof(v->viewport()) == std::addressof(viewport); });
    }

} // namespace retro
