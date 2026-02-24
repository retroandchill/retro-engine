/**
 * @file pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.pipeline_manager;

namespace retro
{
    PipelineManager::PipelineManager(const std::vector<RenderPipeline *> &pipelines)
    {
        for (auto &pipeline : pipelines)
        {
            std::type_index type = pipeline->component_type();
            pipelines_[type] = PipelineUsage{.pipeline = pipeline, .usage_count = 1};
        }
    }

    void PipelineManager::collect_all_draw_calls(const SceneNodeList &nodes,
                                                 const Vector2u viewport_size,
                                                 const Viewport &viewport)
    {
        for (const auto &pipeline :
             pipelines_ | std::views::values |
                 std::views::filter([](const PipelineUsage &usage) { return usage.usage_count > 0; }) |
                 std::views::transform(&PipelineUsage::pipeline))
        {
            pipeline->collect_draw_calls(nodes, viewport_size, viewport);
        }
    }
} // namespace retro
