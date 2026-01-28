/**
 * @file pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    void PipelineManager::collect_all_draw_calls(Scene &registry, const Vector2u viewport_size)
    {
        for (const auto &pipeline :
             pipelines_ | std::views::values |
                 std::views::filter([](const PipelineUsage &usage) { return usage.usage_count > 0; }) |
                 std::views::transform(&PipelineUsage::pipeline))
        {
            pipeline->collect_draw_calls(registry, viewport_size);
        }
    }
} // namespace retro
