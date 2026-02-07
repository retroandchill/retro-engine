/**
 * @file presenter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.scopes.presenter;

import vulkan_hpp;
import retro.platform.window;
import retro.core.functional.delegate;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.sync;
import retro.renderer.vulkan.components.buffer_manager;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.data.device_handles_data;

namespace retro
{
    export struct VulkanPresenterCreateInfo
    {
        Window &window;
        vk::Instance instance;
        vk::SurfaceKHR surface;
        VulkanDeviceHandles device_handles;
        vk::CommandPool command_pool;
        VulkanPipelineManager &pipeline_manager;
        VulkanBufferManager &buffer_manager;
    };

    export class VulkanPresenter
    {
        VulkanPresenter(Window &window,
                        vk::SurfaceKHR surface,
                        VulkanDeviceHandles device_handles,
                        VulkanSwapchain swapchain,
                        vk::UniqueRenderPass render_pass,
                        std::vector<vk::UniqueFramebuffer> framebuffers,
                        std::vector<vk::UniqueCommandBuffer> command_buffers,
                        VulkanSyncObjects sync,
                        VulkanPipelineManager &pipeline_manager,
                        VulkanBufferManager &buffer_manager);

      public:
        static VulkanPresenter create(const VulkanPresenterCreateInfo &info);

        void begin_frame();

        void end_frame();

      private:
        void recreate_swapchain();

        void record_command_buffer(vk::CommandBuffer cmd, std::uint32_t image_index);

        Window &window_;
        vk::SurfaceKHR surface_;
        VulkanDeviceHandles device_handles_;
        VulkanSwapchain swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        std::vector<vk::UniqueCommandBuffer> command_buffers_{};
        VulkanSyncObjects sync_;
        PipelineManager &pipeline_manager_;
        VulkanBufferManager &buffer_manager_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;

        static constexpr std::uint32_t MAX_FRAMES_IN_FLIGHT = 2;
    };
} // namespace retro
