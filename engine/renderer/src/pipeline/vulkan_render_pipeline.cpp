/**
 * @file vulkan_render_pipeline.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cstddef>

module retro.renderer;

import vulkan_hpp;

namespace retro
{
    struct PreparedGeometry
    {
        vk::Buffer vertex_buffer{};
        vk::Buffer index_buffer{};
        uint32 vertex_offset{};
        uint32 index_offset{};
    };

    class VulkanRenderContext final : public RenderContext
    {
      public:
        inline VulkanRenderContext(vk::Pipeline pipeline,
                                   const vk::CommandBuffer cmd,
                                   vk::PipelineLayout pipeline_layout,
                                   const Vector2u viewport_size)
            : pipeline_{pipeline}, cmd_(cmd), pipeline_layout_{pipeline_layout}, viewport_size_(viewport_size)
        {
        }

        void draw_geometry(const std::span<const GeometryDrawCall> geometry) override
        {
            cmd_.bindPipeline(vk::PipelineBindPoint::eGraphics, pipeline_);

            for (auto &d : geometry)
            {

                cmd_.pushConstants(pipeline_layout_,
                                   vk::ShaderStageFlagBits::eVertex | vk::ShaderStageFlagBits::eFragment,
                                   0,
                                   static_cast<uint32>(d.push_constants.size()),
                                   d.push_constants.data());

                auto [vertex_buffer, index_buffer, vertex_offset, index_offset] = prepare_geometry(d.geometry);
                cmd_.bindVertexBuffers(0, {vertex_buffer}, {vertex_offset});
                cmd_.bindIndexBuffer(index_buffer, index_offset, vk::IndexType::eUint32);

                cmd_.drawIndexed(static_cast<uint32>(d.geometry.indices.size()), 1, 0, 0, 0);
            }
        }

        void draw_procedural(const std::span<ProceduralDrawCall> vertex_count) override
        {
            cmd_.bindPipeline(vk::PipelineBindPoint::eGraphics, pipeline_);
            for (auto &d : vertex_count)
            {
                cmd_.pushConstants(pipeline_layout_,
                                   vk::ShaderStageFlagBits::eVertex | vk::ShaderStageFlagBits::eFragment,
                                   0,
                                   static_cast<uint32>(d.push_constants.size()),
                                   d.push_constants.data());

                cmd_.draw(d.vertex_count, 1, 0, 0);
            }
        }

      private:
        static PreparedGeometry prepare_geometry(const BasicGeometry<ArenaAllocator> &geometry)
        {
            const usize vertex_size = geometry.vertices.size() * sizeof(Vertex);
            const usize index_size = geometry.indices.size() * sizeof(uint32);

            auto &buffer_manager = VulkanBufferManager::instance();

            auto v_allocation = buffer_manager.allocate_transient(vertex_size, vk::BufferUsageFlagBits::eVertexBuffer);
            auto i_allocation = buffer_manager.allocate_transient(index_size, vk::BufferUsageFlagBits::eIndexBuffer);

            std::memcpy(v_allocation.mapped_data, geometry.vertices.data(), vertex_size);
            std::memcpy(i_allocation.mapped_data, geometry.indices.data(), index_size);

            return {v_allocation.buffer,
                    i_allocation.buffer,
                    static_cast<uint32>(v_allocation.offset),
                    static_cast<uint32>(i_allocation.offset)};
        }

        vk::Pipeline pipeline_;
        vk::CommandBuffer cmd_{};
        vk::PipelineLayout pipeline_layout_{};
        Vector2u viewport_size_{};
    };

    void VulkanRenderPipeline::clear_draw_queue()
    {
        pipeline_->clear_draw_queue();
    }

    void VulkanRenderPipeline::recreate(vk::Device device, const VulkanSwapchain &swapchain, vk::RenderPass render_pass)
    {
        // Common configuration logic that uses the shader names and
        // potentially other metadata to build the vk::Pipeline
        pipeline_layout_ = create_pipeline_layout(device);
        graphics_pipeline_ = create_graphics_pipeline(device, pipeline_layout_.get(), swapchain, render_pass);
        pipeline_->clear_draw_queue();
    }

    void VulkanRenderPipeline::bind_and_render(const vk::CommandBuffer cmd, const Vector2u viewport_size)
    {
        VulkanRenderContext context{graphics_pipeline_.get(), cmd, pipeline_layout_.get(), viewport_size};
        pipeline_->execute(context);
    }

    vk::UniquePipelineLayout VulkanRenderPipeline::create_pipeline_layout(const vk::Device device) const
    {
        vk::DescriptorSetLayoutBinding sampler_layout_binding{0,
                                                              vk::DescriptorType::eCombinedImageSampler,
                                                              1,
                                                              vk::ShaderStageFlagBits::eFragment};

        vk::DescriptorSetLayoutCreateInfo layout_info{{}, 1, &sampler_layout_binding};

        auto descriptor_set_layout = device.createDescriptorSetLayoutUnique(layout_info);

        vk::PushConstantRange range{vk::ShaderStageFlagBits::eVertex | vk::ShaderStageFlagBits::eFragment,
                                    0,
                                    static_cast<uint32>(pipeline_->push_constants_size())};

        std::array layouts = {descriptor_set_layout.get()};

        const vk::PipelineLayoutCreateInfo pipeline_layout_info{{}, layouts.size(), layouts.data(), 1, &range};
        return device.createPipelineLayoutUnique(pipeline_layout_info);
    }

    vk::UniquePipeline VulkanRenderPipeline::create_graphics_pipeline(vk::Device device,
                                                                      vk::PipelineLayout layout,
                                                                      const VulkanSwapchain &swapchain,
                                                                      vk::RenderPass render_pass)
    {
        vk::VertexInputBindingDescription binding_description{0, sizeof(Vertex), vk::VertexInputRate::eVertex};

        std::array attribute_descriptions = {

            vk::VertexInputAttributeDescription{0, 0, vk::Format::eR32G32Sfloat, offsetof(retro::Vertex, position)},
            vk::VertexInputAttributeDescription{1, 0, vk::Format::eR32G32Sfloat, offsetof(retro::Vertex, uv)},
            vk::VertexInputAttributeDescription{2, 0, vk::Format::eR32G32B32A32Sfloat, offsetof(retro::Vertex, color)}};

        vk::PipelineVertexInputStateCreateInfo vertex_input_info{{},
                                                                 1,
                                                                 &binding_description,
                                                                 static_cast<uint32>(attribute_descriptions.size()),
                                                                 attribute_descriptions.data()};

        auto [vertex_shader, fragment_shader] = pipeline_->shaders();
        auto vert_module = create_shader_module(device, vertex_shader);
        auto frag_module = create_shader_module(device, fragment_shader);

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
                                                     &vertex_input_info,
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

    vk::UniqueShaderModule VulkanRenderPipeline::create_shader_module(const vk::Device device,
                                                                      const std::filesystem::path &path)
    {
        const auto bytes = read_binary_file(path);
        const auto *code = reinterpret_cast<const uint32 *>(bytes.data());

        if (bytes.size() % sizeof(uint32) != 0)
        {
            throw std::runtime_error{"SPIR-V file size is not a multiple of 4 bytes"};
        }

        const vk::ShaderModuleCreateInfo info{{}, bytes.size(), code};
        return device.createShaderModuleUnique(info);
    }
} // namespace retro
