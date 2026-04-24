/**
 * @file pipeline.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.components.pipeline;

import retro.runtime.rendering.render_pipeline;
import retro.core.math.vector;
import vulkan;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.runtime.world.viewport;
import retro.runtime.rendering.draw_command;
import retro.core.memory.small_unique_ptr;
import retro.core.util.noncopyable;

namespace retro
{
    export class VulkanRenderPipeline
    {
      public:
        inline VulkanRenderPipeline(RenderPipeline &pipeline,
                                    VulkanDevice &device,
                                    VulkanBufferManager &buffer_manager,
                                    const vk::Extent2D extent,
                                    const vk::RenderPass render_pass)
            : device_{device}, pipeline_{pipeline}, buffer_manager_{buffer_manager}
        {
            recreate(device, extent, render_pass);
        }

        void recreate(VulkanDevice &device, vk::Extent2D extent, vk::RenderPass render_pass);

        void bind_and_render(vk::CommandBuffer cmd,
                             Vector2u viewport_size,
                             std::span<const DrawCommand> draw_commands,
                             vk::DescriptorPool descriptor_pool);

      private:
        [[nodiscard]] vk::UniquePipelineLayout create_pipeline_layout(VulkanDevice &device);

        vk::UniquePipeline create_graphics_pipeline(VulkanDevice &device,
                                                    vk::PipelineLayout layout,
                                                    vk::Extent2D extent,
                                                    vk::RenderPass render_pass) const;

        static vk::UniqueShaderModule create_shader_module(const VulkanDevice &device,
                                                           const std::filesystem::path &path);

        RenderPipeline &pipeline_;
        VulkanDevice &device_;
        VulkanBufferManager &buffer_manager_;
        vk::UniquePipelineLayout pipeline_layout_;
        vk::UniqueDescriptorSetLayout descriptor_set_layout_;
        vk::UniquePipeline graphics_pipeline_;
    };

    export class VulkanPipelineManager : NonCopyable
    {
      public:
        explicit inline VulkanPipelineManager(VulkanDevice &device, VulkanBufferManager &buffer_manager)
            : device_{device}, buffer_manager_{buffer_manager}, cache_{device_.create_pipeline_cache()}
        {
        }

        void recreate_pipelines(vk::Extent2D extent, vk::RenderPass render_pass);

        void create_pipeline(std::type_index type,
                             RenderPipeline &pipeline,
                             vk::Extent2D extent,
                             vk::RenderPass render_pass);

        void destroy_pipeline(std::type_index type);

        void bind_and_render(vk::CommandBuffer cmd,
                             Vector2u viewport_size,
                             std::span<const SmallUniquePtr<DrawCommandSource>> draw_command_sources,
                             vk::DescriptorPool descriptor_pool);

      private:
        VulkanDevice &device_;
        VulkanBufferManager &buffer_manager_;
        vk::UniquePipelineCache cache_;

        std::map<std::type_index, VulkanRenderPipeline> pipelines_;
    };
} // namespace retro
