/**
 * @file pipeline_registry.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer;

namespace retro
{
    PipelineRegistry::PipelineRegistry()
    {
        factories_[QuadRenderProxy::type_id()] = [](vk::Device device,
                                                    const VulkanSwapchain &swapchain,
                                                    vk::RenderPass render_pass) -> std::unique_ptr<RenderPipeline>
        {
            return std::make_unique<QuadRenderPipeline>(device, swapchain, render_pass);
        };
    }

    PipelineRegistry &PipelineRegistry::instance()
    {
        static PipelineRegistry instance;
        return instance;
    }

    void PipelineRegistry::register_pipeline(const Name name, PipelineFactory factory)
    {
        factories_[name] = std::move(factory);
    }

    void PipelineRegistry::unregister_pipeline(const Name name)
    {
        factories_.erase(name);
    }

    std::vector<std::unique_ptr<RenderPipeline>> PipelineRegistry::create_pipelines(vk::Device device,
                                                                                    const VulkanSwapchain &swapchain,
                                                                                    vk::RenderPass render_pass) const
    {
        return factories_ | std::views::values |
               std::views::transform([&](auto factory) { return factory(device, swapchain, render_pass); }) |
               std::ranges::to<std::vector>();
    }
} // namespace retro
