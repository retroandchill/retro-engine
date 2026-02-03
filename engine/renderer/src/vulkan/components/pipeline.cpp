/**
 * @file pipeline.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.components.pipeline;

import vulkan_hpp;
import retro.core.io.file_stream;
import retro.core.containers.inline_list;
import retro.core.functional.overload;
import retro.runtime.rendering.shader_layout;
import retro.runtime.rendering.draw_command;
import retro.runtime.rendering.texture_render_data;
import retro.renderer.vulkan.data.texture_render_data;

namespace retro
{
    namespace
    {
        constexpr vk::VertexInputRate to_vulkan_enum(const VertexInputType type)
        {
            switch (type)
            {
                case VertexInputType::Vertex:
                    return vk::VertexInputRate::eVertex;
                case VertexInputType::Instance:
                    return vk::VertexInputRate::eInstance;
                default:
                    throw std::invalid_argument("Invalid vertex input type");
            }
        }

        constexpr vk::Format to_vulkan_enum(const ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType::Int32:
                    return vk::Format::eR32Sint;
                case ShaderDataType::Uint32:
                    return vk::Format::eR32Uint;
                case ShaderDataType::Float:
                    return vk::Format::eR32Sfloat;
                case ShaderDataType::Vec2:
                    return vk::Format::eR32G32Sfloat;
                case ShaderDataType::Vec3:
                    return vk::Format::eR32G32B32Sfloat;
                case ShaderDataType::Vec4:
                    return vk::Format::eR32G32B32A32Sfloat;
                default:
                    throw std::invalid_argument("Invalid shader data type");
            }
        }

        constexpr vk::DescriptorType to_vulkan_enum(const DescriptorType type)
        {
            switch (type)
            {
                case DescriptorType::Sampler:
                    return vk::DescriptorType::eSampler;
                case DescriptorType::CombinedImageSampler:
                    return vk::DescriptorType::eCombinedImageSampler;
                case DescriptorType::UniformBuffer:
                    return vk::DescriptorType::eUniformBuffer;
                case DescriptorType::StorageBuffer:
                    return vk::DescriptorType::eStorageBuffer;
                default:
                    throw std::invalid_argument("Invalid descriptor type");
            }
        }

        constexpr vk::Flags<vk::ShaderStageFlagBits> to_vulkan_enum(const ShaderStage stage)
        {
            vk::Flags<vk::ShaderStageFlagBits> result{};
            if (has_flag(stage, ShaderStage::Vertex))
            {
                result |= vk::ShaderStageFlagBits::eVertex;
            }

            if (has_flag(stage, ShaderStage::Fragment))
            {
                result |= vk::ShaderStageFlagBits::eFragment;
            }

            return result;
        }
    } // namespace

    struct PreparedGeometry
    {
        vk::Buffer vertex_buffer{};
        vk::Buffer index_buffer{};
        std::uint32_t vertex_offset{};
        std::uint32_t index_offset{};
    };

    class VulkanRenderContext final : public RenderContext
    {
      public:
        inline VulkanRenderContext(vk::Device device,
                                   vk::Pipeline pipeline,
                                   const vk::CommandBuffer cmd,
                                   vk::PipelineLayout pipeline_layout,
                                   vk::DescriptorSetLayout descriptor_set_layout,
                                   vk::DescriptorPool descriptor_pool,
                                   VulkanBufferManager &buffer_manager,
                                   const Vector2u viewport_size)
            : device_{device}, pipeline_{pipeline}, cmd_(cmd), pipeline_layout_{pipeline_layout},
              descriptor_set_layout_{descriptor_set_layout}, descriptor_pool_{descriptor_pool},
              buffer_manager_(buffer_manager), viewport_size_(viewport_size)
        {
        }

        void draw(const std::span<const DrawCommand> draw_commands, const ShaderLayout &layout) override
        {
            cmd_.bindPipeline(vk::PipelineBindPoint::eGraphics, pipeline_);

            for (auto &batch : draw_commands)
            {
                queue_draw_command(batch, layout);
            }
        }

      private:
        void queue_draw_command(const DrawCommand &command, const ShaderLayout &layout) const
        {
            bind_vertex_buffers(command, layout);
            bind_index_buffer(command);
            bind_descriptor_sets(command, layout);
            bind_push_constants(command, layout);

            if (command.index_buffer.empty())
            {
                cmd_.draw(static_cast<std::uint32_t>(command.index_count),
                          static_cast<std::int32_t>(command.instance_count),
                          0,
                          0);
            }
            else
            {
                cmd_.drawIndexed(static_cast<std::uint32_t>(command.index_count),
                                 static_cast<std::int32_t>(command.instance_count),
                                 0,
                                 0,
                                 0);
            }
        }

        void bind_vertex_buffers(const DrawCommand &command, const ShaderLayout &layout) const
        {
            if (layout.vertex_bindings.empty())
                return;

            InlineList<vk::Buffer, DRAW_ARRAY_SIZE> vertex_buffers;
            InlineList<std::size_t, DRAW_ARRAY_SIZE> offsets;
            std::size_t vertex_binding = 0;
            std::size_t instance_binding = 0;
            for (auto &binding : layout.vertex_bindings)
            {
                if (binding.type == VertexInputType::Vertex)
                {
                    auto &vertex_buffer = command.vertex_buffers[vertex_binding];
                    auto [buffer, mapped_data, offset] =
                        buffer_manager_.allocate_transient(static_cast<std::uint32_t>(vertex_buffer.size()),
                                                           vk::BufferUsageFlagBits::eVertexBuffer);
                    std::memcpy(mapped_data, vertex_buffer.data(), vertex_buffer.size());
                    vertex_buffers.push_back(buffer);
                    offsets.push_back(offset);
                    vertex_binding++;
                }
                else if (binding.type == VertexInputType::Instance)
                {
                    auto &instance_buffer = command.instance_buffers[instance_binding];
                    auto [buffer, mapped_data, offset] =
                        buffer_manager_.allocate_transient(static_cast<std::uint32_t>(instance_buffer.size()),
                                                           vk::BufferUsageFlagBits::eVertexBuffer);
                    std::memcpy(mapped_data, instance_buffer.data(), instance_buffer.size());
                    vertex_buffers.push_back(buffer);
                    offsets.push_back(offset);
                    instance_binding++;
                }
            }

            cmd_.bindVertexBuffers(0, vertex_buffers, offsets);
        }

        void bind_descriptor_sets(const DrawCommand &command, const ShaderLayout &layout) const
        {
            if (layout.descriptor_bindings.empty())
                return;

            vk::DescriptorSetAllocateInfo alloc_info{};
            alloc_info.descriptorPool = descriptor_pool_;
            alloc_info.descriptorSetCount = static_cast<std::uint32_t>(layout.descriptor_bindings.size());
            alloc_info.pSetLayouts = &descriptor_set_layout_;

            auto descriptor_sets = device_.allocateDescriptorSets(alloc_info);

            InlineList<vk::DescriptorBufferInfo, DRAW_ARRAY_SIZE> buffer_infos;
            InlineList<vk::DescriptorImageInfo, DRAW_ARRAY_SIZE> image_infos;

            InlineList<vk::WriteDescriptorSet, DRAW_ARRAY_SIZE> writes;
            for (auto &&[i, binding] : layout.descriptor_bindings | std::views::enumerate)
            {
                auto &write_set = writes.emplace_back();
                write_set.descriptorCount = 1;
                write_set.dstBinding = 0;
                write_set.dstSet = descriptor_sets[i];

                std::visit(Overload{[&](const std::span<const std::byte> descriptor_data)
                                    {
                                        auto [buffer, mapped_data, offset] = buffer_manager_.allocate_transient(
                                            static_cast<std::uint32_t>(descriptor_data.size()),
                                            vk::BufferUsageFlagBits::eStorageBuffer);
                                        write_set.descriptorType = vk::DescriptorType::eStorageBuffer;

                                        std::memcpy(mapped_data,
                                                    descriptor_data.data(),
                                                    static_cast<std::uint32_t>(descriptor_data.size()));

                                        // Bind instance buffer to descriptor set
                                        const auto &buffer_info = buffer_infos.emplace_back(
                                            buffer,
                                            offset,
                                            static_cast<std::uint32_t>(descriptor_data.size()));
                                        write_set.pBufferInfo = &buffer_info;
                                    },
                                    [&](const TextureRenderData *render_data)
                                    {
                                        auto &textureData = dynamic_cast<const VulkanTextureRenderData &>(*render_data);
                                        write_set.descriptorType = vk::DescriptorType::eCombinedImageSampler;

                                        const auto &img_info =
                                            image_infos.emplace_back(textureData.sampler(),
                                                                     textureData.view(),
                                                                     vk::ImageLayout::eShaderReadOnlyOptimal);

                                        write_set.pImageInfo = &img_info;
                                    }},
                           command.descriptor_sets[i]);
            }

            device_.updateDescriptorSets(writes.size(), writes.data(), 0, nullptr);

            // Bind the descriptor set to the graphics pipeline
            cmd_.bindDescriptorSets(vk::PipelineBindPoint::eGraphics,
                                    pipeline_layout_,
                                    0,                                                  // first set
                                    static_cast<std::uint32_t>(descriptor_sets.size()), // descriptor set count
                                    descriptor_sets.data(),
                                    0, // dynamic offset count
                                    nullptr);
        }

        void bind_index_buffer(const DrawCommand &command) const
        {
            auto &index_buffer = command.index_buffer;
            if (index_buffer.empty())
                return;
            auto [buffer, mapped_data, offset] =
                buffer_manager_.allocate_transient(static_cast<std::uint32_t>(index_buffer.size()),
                                                   vk::BufferUsageFlagBits::eIndexBuffer);
            std::memcpy(mapped_data, index_buffer.data(), index_buffer.size());
            cmd_.bindIndexBuffer(buffer, offset, vk::IndexType::eUint32);
        }

        void bind_push_constants(const DrawCommand &command, const ShaderLayout &layout) const
        {
            if (!layout.push_constant_bindings.has_value())
                return;

            cmd_.pushConstants(pipeline_layout_,
                               to_vulkan_enum(layout.push_constant_bindings->stages),
                               0,
                               static_cast<std::uint32_t>(command.push_constants.size()),
                               command.push_constants.data());
        }

        vk::Device device_{};
        vk::Pipeline pipeline_;
        vk::CommandBuffer cmd_{};
        vk::PipelineLayout pipeline_layout_{};
        vk::DescriptorSetLayout descriptor_set_layout_{};
        vk::DescriptorPool descriptor_pool_{};
        VulkanBufferManager &buffer_manager_;
        Vector2u viewport_size_{};
    };

    void VulkanRenderPipeline::clear_draw_queue()
    {
        pipeline_->clear_draw_queue();
    }

    void VulkanRenderPipeline::recreate(vk::Device device,
                                        const VulkanSwapchain &swapchain,
                                        const vk::RenderPass render_pass)
    {
        pipeline_layout_ = create_pipeline_layout(device);
        graphics_pipeline_ = create_graphics_pipeline(device, pipeline_layout_.get(), swapchain, render_pass);
        pipeline_->clear_draw_queue();
    }

    void VulkanRenderPipeline::bind_and_render(const vk::CommandBuffer cmd,
                                               const Vector2u viewport_size,
                                               const vk::DescriptorPool descriptor_pool,
                                               VulkanBufferManager &buffer_manager)
    {
        VulkanRenderContext context{device_,
                                    graphics_pipeline_.get(),
                                    cmd,
                                    pipeline_layout_.get(),
                                    descriptor_set_layout_.get(),
                                    descriptor_pool,
                                    buffer_manager,
                                    viewport_size};
        pipeline_->execute(context);
    }

    vk::UniquePipelineLayout VulkanRenderPipeline::create_pipeline_layout(const vk::Device device)
    {
        auto &shader_layout = pipeline_->shaders();

        std::vector<vk::DescriptorSetLayoutBinding> bindings;
        bindings.reserve(shader_layout.descriptor_bindings.size());

        for (const auto [index, binding] : shader_layout.descriptor_bindings | std::views::enumerate)
        {
            bindings.emplace_back(static_cast<std::uint32_t>(index),
                                  to_vulkan_enum(binding.type),
                                  static_cast<std::uint32_t>(binding.count),
                                  to_vulkan_enum(binding.stages));
        }

        const vk::DescriptorSetLayoutCreateInfo layout_info{.bindingCount = static_cast<std::uint32_t>(bindings.size()),
                                                            .pBindings = bindings.data()};
        descriptor_set_layout_ = device.createDescriptorSetLayoutUnique(layout_info);
        std::array layouts = {descriptor_set_layout_.get()};

        InlineList<vk::PushConstantRange, 1> push_constant_ranges;
        for (const auto [stages, size, offset] : shader_layout.push_constant_bindings)
        {
            push_constant_ranges.emplace_back(to_vulkan_enum(stages),
                                              static_cast<std::uint32_t>(offset),
                                              static_cast<std::uint32_t>(size));
        }

        const vk::PipelineLayoutCreateInfo pipeline_layout_info{
            .setLayoutCount = layouts.size(),
            .pSetLayouts = layouts.data(),
            .pushConstantRangeCount = static_cast<std::uint32_t>(push_constant_ranges.size()),
            .pPushConstantRanges = push_constant_ranges.data()};
        return device.createPipelineLayoutUnique(pipeline_layout_info);
    }

    vk::UniquePipeline VulkanRenderPipeline::create_graphics_pipeline(vk::Device device,
                                                                      vk::PipelineLayout layout,
                                                                      const VulkanSwapchain &swapchain,
                                                                      vk::RenderPass render_pass)
    {
        auto &shader_layout = pipeline_->shaders();

        std::vector<vk::VertexInputBindingDescription> binding_descriptions;
        std::vector<vk::VertexInputAttributeDescription> attribute_descriptions;

        binding_descriptions.reserve(shader_layout.vertex_bindings.size());
        std::size_t attribute_count = std::ranges::fold_left(
            shader_layout.vertex_bindings |
                std::views::transform([](const VertexInputBinding &b) { return b.attributes.size(); }),
            0,
            std::plus{});
        attribute_descriptions.reserve(attribute_count);

        for (const auto [bind_index, binding] : shader_layout.vertex_bindings | std::views::enumerate)
        {
            binding_descriptions.emplace_back(static_cast<std::uint32_t>(bind_index),
                                              static_cast<std::uint32_t>(binding.stride),
                                              to_vulkan_enum(binding.type));

            for (const auto &attribute : binding.attributes)
            {
                attribute_descriptions.emplace_back(static_cast<std::uint32_t>(attribute_descriptions.size()),
                                                    binding_descriptions.back().binding,
                                                    to_vulkan_enum(attribute.type),
                                                    attribute.offset);
            }
        }

        vk::PipelineVertexInputStateCreateInfo vertex_input_info{
            .vertexBindingDescriptionCount = static_cast<std::uint32_t>(binding_descriptions.size()),
            .pVertexBindingDescriptions = binding_descriptions.data(),
            .vertexAttributeDescriptionCount = static_cast<std::uint32_t>(attribute_descriptions.size()),
            .pVertexAttributeDescriptions = attribute_descriptions.data()};

        auto vert_module = create_shader_module(device, shader_layout.vertex_shader);
        auto frag_module = create_shader_module(device, shader_layout.fragment_shader);

        vk::PipelineShaderStageCreateInfo vert_stage{.stage = vk::ShaderStageFlagBits::eVertex,
                                                     .module = vert_module.get(),
                                                     .pName = "main"};

        vk::PipelineShaderStageCreateInfo frag_stage{.stage = vk::ShaderStageFlagBits::eFragment,
                                                     .module = frag_module.get(),
                                                     .pName = "main"};

        std::array shader_stages = {vert_stage, frag_stage};

        vk::PipelineInputAssemblyStateCreateInfo input_assembly{.topology = vk::PrimitiveTopology::eTriangleList,
                                                                .primitiveRestartEnable = vk::False};

        vk::Viewport viewport{.x = 0.0f,
                              .y = 0.0f,
                              .width = static_cast<float>(swapchain.extent().width),
                              .height = static_cast<float>(swapchain.extent().height),
                              .minDepth = 0.0f,
                              .maxDepth = 1.0f};

        vk::Rect2D scissor{.offset = {.x = 0, .y = 0}, .extent = swapchain.extent()};

        vk::PipelineViewportStateCreateInfo viewport_state{.viewportCount = 1,
                                                           .pViewports = &viewport,
                                                           .scissorCount = 1,
                                                           .pScissors = &scissor};

        vk::PipelineRasterizationStateCreateInfo rasterizer{.depthClampEnable = vk::False,
                                                            .rasterizerDiscardEnable = vk::False,
                                                            .polygonMode = vk::PolygonMode::eFill,
                                                            .cullMode = vk::CullModeFlagBits::eBack,
                                                            .frontFace = vk::FrontFace::eCounterClockwise,
                                                            .depthBiasEnable = vk::False,
                                                            .depthBiasConstantFactor = 0.0f,
                                                            .depthBiasClamp = 0.0f,
                                                            .depthBiasSlopeFactor = 0.0f,
                                                            .lineWidth = 1.0f};

        vk::PipelineMultisampleStateCreateInfo multisampling{.rasterizationSamples = vk::SampleCountFlagBits::e1,
                                                             .sampleShadingEnable = vk::False};

        vk::PipelineColorBlendAttachmentState color_blend_attachment{
            .blendEnable = vk::True,
            .srcColorBlendFactor = vk::BlendFactor::eSrcAlpha,
            .dstColorBlendFactor = vk::BlendFactor::eOneMinusSrcAlpha,
            .colorBlendOp = vk::BlendOp::eAdd,
            .srcAlphaBlendFactor = vk::BlendFactor::eOne,
            .dstAlphaBlendFactor = vk::BlendFactor::eOneMinusSrcAlpha,
            .alphaBlendOp = vk::BlendOp::eAdd,
            .colorWriteMask = vk::ColorComponentFlagBits::eR | vk::ColorComponentFlagBits::eG |
                              vk::ColorComponentFlagBits::eB | vk::ColorComponentFlagBits::eA};

        vk::PipelineColorBlendStateCreateInfo color_blending{.logicOpEnable = vk::False,
                                                             .logicOp = vk::LogicOp::eCopy,
                                                             .attachmentCount = 1,
                                                             .pAttachments = &color_blend_attachment};

        vk::GraphicsPipelineCreateInfo pipeline_info{.stageCount = static_cast<std::uint32_t>(shader_stages.size()),
                                                     .pStages = shader_stages.data(),
                                                     .pVertexInputState = &vertex_input_info,
                                                     .pInputAssemblyState = &input_assembly,
                                                     .pViewportState = &viewport_state,
                                                     .pRasterizationState = &rasterizer,
                                                     .pMultisampleState = &multisampling,
                                                     .pColorBlendState = &color_blending,
                                                     .layout = layout,
                                                     .renderPass = render_pass,
                                                     .subpass = 0};

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
        const auto *code = reinterpret_cast<const std::uint32_t *>(bytes.data());

        if (bytes.size() % sizeof(std::uint32_t) != 0)
        {
            throw std::runtime_error{"SPIR-V file size is not a multiple of 4 bytes"};
        }

        const vk::ShaderModuleCreateInfo info{.codeSize = bytes.size(), .pCode = code};
        return device.createShaderModuleUnique(info);
    }

    void VulkanPipelineManager::recreate_pipelines(const VulkanSwapchain &swapchain, const vk::RenderPass render_pass)
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.recreate(device_, swapchain, render_pass);
        }
    }

    void VulkanPipelineManager::create_pipeline(std::type_index type,
                                                std::shared_ptr<RenderPipeline> pipeline,
                                                const VulkanSwapchain &swapchain,
                                                vk::RenderPass render_pass)
    {
        pipelines_.emplace_back(std::move(pipeline), device_, swapchain, render_pass);
        pipeline_indices_.emplace(type, pipelines_.size() - 1);
    }

    void VulkanPipelineManager::destroy_pipeline(const std::type_index type)
    {
        if (const auto it = pipeline_indices_.find(type); it != pipeline_indices_.end())
        {
            pipelines_.erase(std::next(pipelines_.begin(), static_cast<std::ptrdiff_t>(it->second)));
            pipeline_indices_.erase(type);
        }
    }

    void VulkanPipelineManager::bind_and_render(const vk::CommandBuffer cmd,
                                                const Vector2u viewport_size,
                                                vk::DescriptorPool descriptor_pool,
                                                VulkanBufferManager &buffer_manager)
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.bind_and_render(cmd, viewport_size, descriptor_pool, buffer_manager);
        }
    }

    void VulkanPipelineManager::clear_draw_queue()
    {
        for (auto &pipeline : pipelines_)
        {
            pipeline.clear_draw_queue();
        }
    }
} // namespace retro
