/**
 * @file pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    void PipelineManager::collect_all_draw_calls(const entt::registry &registry, const Vector2u viewport_size)
    {
        for (const auto &pipeline : active_pipelines_ | std::views::values)
        {
            pipeline->collect_draw_calls(registry, viewport_size);
        }
    }
} // namespace retro
