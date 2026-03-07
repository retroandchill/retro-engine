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

    DrawCommandSet PipelineManager::collect_draw_command_sources(const SceneNodeList &nodes,
                                                                 Vector2u viewport_size,
                                                                 const Viewport &viewport,
                                                                 std::pmr::memory_resource &memory_resource)
    {
        return DrawCommandSet{
            .layout = viewport.screen_layout(),
            .z_order = viewport.z_order(),
            .sources =
                pipelines_ | std::views::values |
                std::views::filter([](const PipelineUsage &usage) { return usage.usage_count > 0; }) |
                std::views::transform(&PipelineUsage::pipeline) |
                std::views::transform(
                    [&nodes, &viewport_size, &viewport, &memory_resource](RenderPipeline *pipeline)
                    { return pipeline->collect_draw_calls_source(nodes, viewport_size, viewport, memory_resource); }) |
                std::ranges::to<std::pmr::vector<SmallUniquePtr<DrawCommandSource>>>(&memory_resource)};
    }
} // namespace retro
