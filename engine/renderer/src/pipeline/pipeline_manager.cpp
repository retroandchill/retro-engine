/**
 * @file pipeline_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

namespace retro
{
    void PipelineManager::bind_and_render(const vk::CommandBuffer cmd, const Vector2u viewport_size) const
    {
        for (const auto &pipeline : pipelines_)
        {
            pipeline->bind_and_render(cmd, viewport_size);
        }
    }

    void PipelineManager::clear_draw_queue()
    {
        for (const auto &pipeline : pipelines_)
        {
            pipeline->clear_draw_queue();
        }
    }
} // namespace retro
