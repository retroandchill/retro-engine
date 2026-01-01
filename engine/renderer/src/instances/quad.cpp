//
// Created by fcors on 12/31/2025.
//
module retro.renderer;

namespace retro
{
    struct QuadData
    {
        Color color{};
        Vector2f position{};
        Vector2f size{};
        Vector2f viewport_size{};
    };

    void QuadRenderComponent::create_render_proxy(RenderProxyManager &proxy_manager)
    {
        proxy_manager.emplace_proxy<QuadRenderProxy>(id(), *this);
    }

    void QuadRenderComponent::destroy_render_proxy(RenderProxyManager &proxy_manager)
    {
        proxy_manager.remove_proxy<QuadRenderProxy>(id());
    }

    const Name QuadRenderProxy::TYPE_ID = u"quad"_name;

    Quad QuadRenderProxy::get_draw_call() const
    {
        return Quad{.position = component_->entity().transform().position,
                    .size = component_->size(),
                    .color = component_->color()};
    }

    void QuadRenderPipeline::recreate(const vk::Device device,
                                      const VulkanSwapchain &swapchain,
                                      const vk::RenderPass render_pass)
    {
        pipeline_layout_ = create_pipeline_layout(device);
        graphics_pipeline_ = create_graphics_pipeline(device, pipeline_layout_.get(), swapchain, render_pass);
        pending_quads_.clear();
    }

    void QuadRenderPipeline::queue_draw_calls(const std::any &render_data)
    {
        for (auto &quads = std::any_cast<const std::vector<Quad> &>(render_data);
             const auto &[position, size, color] : quads)
        {
            pending_quads_.emplace_back(position, size, color);
        }
    }

    void QuadRenderPipeline::draw_quad(Vector2f position, Vector2f size, Color color)
    {
        pending_quads_.emplace_back(position, size, color);
    }

    void QuadRenderPipeline::bind_and_render(const vk::CommandBuffer cmd, const Vector2u viewport_size)
    {
        cmd.bindPipeline(vk::PipelineBindPoint::eGraphics, graphics_pipeline_.get());

        for (const auto &[position, size, color] : pending_quads_)
        {
            QuadData push{.color = color,
                          .position = position,
                          .size = size,
                          .viewport_size = {static_cast<float>(viewport_size.x), static_cast<float>(viewport_size.y)}};

            cmd.pushConstants(pipeline_layout_.get(),
                              vk::ShaderStageFlagBits::eVertex | vk::ShaderStageFlagBits::eFragment,
                              0,
                              sizeof(QuadData),
                              &push);

            cmd.draw(6, 1, 0, 0);
        }
    }

    void QuadRenderPipeline::clear_draw_queue()
    {
        pending_quads_.clear();
    }

    vk::UniquePipelineLayout QuadRenderPipeline::create_pipeline_layout(vk::Device device)
    {
        vk::PushConstantRange range{vk::ShaderStageFlagBits::eVertex | vk::ShaderStageFlagBits::eFragment,
                                    0,
                                    sizeof(QuadData)};

        const vk::PipelineLayoutCreateInfo pipeline_layout_info{{}, 0, nullptr, 1, &range};
        return device.createPipelineLayoutUnique(pipeline_layout_info);
    }

    vk::UniquePipeline QuadRenderPipeline::create_graphics_pipeline(vk::Device device,
                                                                    vk::PipelineLayout layout,
                                                                    const VulkanSwapchain &swapchain,
                                                                    vk::RenderPass render_pass)
    {
        // No vertex buffers: all positions come from gl_VertexIndex
        vk::PipelineVertexInputStateCreateInfo vertex_input{};

        auto vert_module = create_shader_module(device, "shaders/fullscreen_quad.vert.spv");
        auto frag_module = create_shader_module(device, "shaders/solid_color.frag.spv");

        vk::PipelineShaderStageCreateInfo vert_stage{{}, vk::ShaderStageFlagBits::eVertex, vert_module.get(), "main"};

        vk::PipelineShaderStageCreateInfo frag_stage{{}, vk::ShaderStageFlagBits::eFragment, frag_module.get(), "main"};

        std::array shader_stages = {vert_stage, frag_stage};

        vk::PipelineInputAssemblyStateCreateInfo input_assembly{{}, vk::PrimitiveTopology::eTriangleList, vk::False};

        vk::Viewport viewport{0.0f,
                              0.0f,
                              static_cast<float>(swapchain.extent().width),
                              static_cast<float>(swapchain.extent().height),
                              0.0f,
                              1.0f};

        vk::Rect2D scissor{{0, 0}, swapchain.extent()};

        vk::PipelineViewportStateCreateInfo viewport_state{{}, 1, &viewport, 1, &scissor};

        vk::PipelineRasterizationStateCreateInfo rasterizer{{},
                                                            vk::False,
                                                            vk::False,
                                                            vk::PolygonMode::eFill,
                                                            vk::CullModeFlagBits::eBack,
                                                            vk::FrontFace::eCounterClockwise,
                                                            vk::False,
                                                            0.0f,
                                                            0.0f,
                                                            0.0f,
                                                            1.0f};

        vk::PipelineMultisampleStateCreateInfo multisampling{{}, vk::SampleCountFlagBits::e1, vk::False};

        vk::PipelineColorBlendAttachmentState color_blend_attachment{
            vk::False,
            vk::BlendFactor::eOne,
            vk::BlendFactor::eZero,
            vk::BlendOp::eAdd,
            vk::BlendFactor::eOne,
            vk::BlendFactor::eZero,
            vk::BlendOp::eAdd,
            vk::ColorComponentFlagBits::eR | vk::ColorComponentFlagBits::eG | vk::ColorComponentFlagBits::eB |
                vk::ColorComponentFlagBits::eA};

        vk::PipelineColorBlendStateCreateInfo color_blending{{},
                                                             vk::False,
                                                             vk::LogicOp::eCopy,
                                                             1,
                                                             &color_blend_attachment};

        vk::GraphicsPipelineCreateInfo pipeline_info{{},
                                                     static_cast<uint32>(shader_stages.size()),
                                                     shader_stages.data(),
                                                     &vertex_input,
                                                     &input_assembly,
                                                     nullptr,
                                                     &viewport_state,
                                                     &rasterizer,
                                                     &multisampling,
                                                     nullptr,
                                                     &color_blending,
                                                     nullptr,
                                                     layout,
                                                     render_pass,
                                                     0};

        auto [result, pipeline] = device.createGraphicsPipelineUnique(nullptr, pipeline_info);
        if (result != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to create graphics pipeline"};
        }

        return std::move(pipeline);
    }

    vk::UniqueShaderModule QuadRenderPipeline::create_shader_module(vk::Device device,
                                                                    const std::filesystem::path &path)
    {
        const auto bytes = read_binary_file(path);
        const auto *code = std::bit_cast<const uint32 *>(bytes.data());

        if (bytes.size() % sizeof(uint32) != 0)
        {
            throw std::runtime_error{"SPIR-V file size is not a multiple of 4 bytes"};
        }

        const vk::ShaderModuleCreateInfo info{{}, bytes.size(), code};
        return device.createShaderModuleUnique(info);
    }
} // namespace retro
