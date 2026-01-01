//
// Created by fcors on 12/31/2025.
//

module retro.renderer;

namespace retro
{
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
