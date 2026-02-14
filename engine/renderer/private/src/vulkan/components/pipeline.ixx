/**
 * @file pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.pipeline;

import retro.runtime.rendering.render_pipeline;
import retro.core.math.vector;
import retro.core.di;
import vulkan_hpp;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.swapchain;
import retro.renderer.vulkan.components.buffer_manager;
import retro.runtime.world.viewport;

namespace retro
{
    export class VulkanRenderPipeline
    {
      public:
        inline VulkanRenderPipeline(RenderPipeline &pipeline,
                                    vk::Device device,
                                    const VulkanSwapchain &swapchain,
                                    vk::RenderPass render_pass)
            : device_{device}, pipeline_{std::addressof(pipeline)}
        {
            recreate(device, swapchain, render_pass);
        }

        void clear_draw_queue();

        void recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass);

        void bind_and_render(vk::CommandBuffer cmd,
                             Vector2u viewport_size,
                             const Viewport &viewport,
                             vk::DescriptorPool descriptor_pool,
                             VulkanBufferManager &buffer_manager);

      private:
        [[nodiscard]] vk::UniquePipelineLayout create_pipeline_layout(vk::Device device);

        vk::UniquePipeline create_graphics_pipeline(vk::Device device,
                                                    vk::PipelineLayout layout,
                                                    const VulkanSwapchain &swapchain,
                                                    vk::RenderPass render_pass);

        static vk::UniqueShaderModule create_shader_module(vk::Device device, const std::filesystem::path &path);

        RenderPipeline *pipeline_;
        vk::Device device_;
        vk::UniquePipelineLayout pipeline_layout_;
        vk::UniqueDescriptorSetLayout descriptor_set_layout_;
        vk::UniquePipeline graphics_pipeline_;
    };

    export class VulkanPipelineManager
    {
      public:
        using Dependencies = TypeList<VulkanDevice>;

        explicit inline VulkanPipelineManager(const VulkanDevice &device)
            : device_{device.device()}, cache_{device_.createPipelineCacheUnique(vk::PipelineCacheCreateInfo{})}
        {
        }

        void recreate_pipelines(const VulkanSwapchain &swapchain, vk::RenderPass render_pass);

        void create_pipeline(std::type_index type,
                             RenderPipeline &pipeline,
                             const VulkanSwapchain &swapchain,
                             vk::RenderPass render_pass);
        void destroy_pipeline(std::type_index type);

        void bind_and_render(vk::CommandBuffer cmd,
                             Vector2u viewport_size,
                             const Viewport &viewport,
                             vk::DescriptorPool descriptor_pool,
                             VulkanBufferManager &buffer_manager);
        void clear_draw_queue();

      private:
        vk::Device device_;
        vk::UniquePipelineCache cache_;

        std::vector<VulkanRenderPipeline> pipelines_;
        std::map<std::type_index, std::size_t> pipeline_indices_;
    };
} // namespace retro
