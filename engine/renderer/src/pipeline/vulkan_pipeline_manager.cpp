/**
 * @file vulkan_pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

namespace retro
{
    void VulkanPipelineManager::recreate_pipelines(const VulkanSwapchain &swapchain, const vk::RenderPass render_pass)
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.recreate(device_, swapchain, render_pass);
        }
    }

    void VulkanPipelineManager::create_pipeline(std::type_index type,
                                                std::shared_ptr<RenderPipeline> pipeline,
                                                const VulkanSwapchain &swapchain,
                                                vk::RenderPass render_pass)
    {
        pipelines_.emplace_back(std::move(pipeline), device_, swapchain, render_pass);
        pipeline_indices_.emplace(type, pipelines_.size() - 1);
    }

    void VulkanPipelineManager::destroy_pipeline(const std::type_index type)
    {
        if (const auto it = pipeline_indices_.find(type); it != pipeline_indices_.end())
        {
            pipelines_.erase(pipelines_.begin() + it->second);
            pipeline_indices_.erase(type);
        }
    }

    void VulkanPipelineManager::bind_and_render(const vk::CommandBuffer cmd, const Vector2u viewport_size)
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.bind_and_render(cmd, viewport_size);
        }
    }

    void VulkanPipelineManager::clear_draw_queue()
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.clear_draw_queue();
        }
    }
} // namespace retro
