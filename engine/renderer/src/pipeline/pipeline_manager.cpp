//
// Created by fcors on 12/31/2025.
//
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
} // namespace retro
