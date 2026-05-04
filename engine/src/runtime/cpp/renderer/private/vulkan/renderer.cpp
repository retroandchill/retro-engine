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
    namespace
    {
        std::atomic<std::uint64_t> next_render_target_{1};

        std::shared_ptr<VulkanRenderTarget> create_render_target(std::unique_ptr<Window> window,
                                                                 vk::UniqueSurfaceKHR surface,
                                                                 VulkanDevice &device,
                                                                 VulkanPipelineManager &pipeline_manager)
        {
            return std::make_shared<VulkanWindowRenderTarget>(next_render_target_.fetch_add(1),
                                                              std::move(window),
                                                              std::move(surface),
                                                              device,
                                                              pipeline_manager);
        }
    } // namespace

    VulkanRenderer2D::VulkanRenderer2D(VulkanRenderBackend &backend,
                                       std::unique_ptr<Window> window,
                                       vk::UniqueSurfaceKHR surface,
                                       VulkanDevice &device,
                                       VulkanBufferManager &buffer_manager,
                                       const vk::CommandPool command_pool)
        : backend_{backend.shared_from_this()},
          render_target_{create_render_target(std::move(window), std::move(surface), device, pipeline_manager_)},
          device_{device}, buffer_manager_{buffer_manager}, command_pool_{command_pool},
          pipeline_manager_{device_, buffer_manager_},
          presenter_{*render_target_, device_, command_pool, pipeline_manager_}
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

    std::shared_ptr<RenderTarget> VulkanRenderer2D::render_target() const
    {
        return render_target_;
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
