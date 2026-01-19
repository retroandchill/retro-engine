/**
 * @file pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

namespace retro
{
    void PipelineManager::bind_and_render(const vk::CommandBuffer cmd, const Vector2u viewport_size)
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.bind_and_render(cmd, viewport_size);
        }
    }

    void PipelineManager::clear_draw_queue()
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.clear_draw_queue();
        }
    }

    std::vector<VulkanRenderPipeline> PipelineManager::create_pipelines(vk::Device device,
                                                                        const VulkanSwapchain &swapchain,
                                                                        const vk::RenderPass render_pass)
    {
        std::vector<VulkanRenderPipeline> pipelines;

        for (auto unique_pipelines = RenderTypeRegistry::instance().create_pipelines();
             auto &pipeline : unique_pipelines)
        {
            pipelines.emplace_back(std::move(pipeline), device, swapchain, render_pass);
        }

        return pipelines;
    }
} // namespace retro
