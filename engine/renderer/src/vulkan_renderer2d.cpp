//
// Created by fcors on 12/26/2025.
//
module;

#include <SDL3/SDL_vulkan.h>
#include <vulkan/vulkan.hpp>

module retro.renderer;

import retro.core;

namespace retro
{
    VulkanRenderer2D::VulkanRenderer2D(std::shared_ptr<VulkanViewport> viewport)
        : viewport_{std::move(viewport)}, instance_{create_instance()},
          surface_{viewport_->create_surface(instance_.get())}, device_{instance_.get(), surface_.get()},
          swapchain_(SwapchainConfig{
              .physical_device = device_.physical_device(),
              .device = device_.device(),
              .surface = surface_.get(),
              .graphics_family = device_.graphics_family_index(),
              .present_family = device_.present_family_index(),
              .width = viewport_->width(),
              .height = viewport_->height(),
          }),
          render_pass_(create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1)),
          framebuffers_(create_framebuffers(device_.device(), render_pass_.get(), swapchain_)),
          command_pool_(CommandPoolConfig{
              .device = device_.device(),
              .queue_family_idx = device_.graphics_family_index(),
              .buffer_count = MAX_FRAMES_IN_FLIGHT,
          }),
          sync_(SyncConfig{
              .device = device_.device(),
              .frames_in_flight = MAX_FRAMES_IN_FLIGHT,
              .swapchain_image_count = static_cast<uint32>(swapchain_.image_views().size()),
          })
    {
        create_pipeline();
    }

    VulkanRenderer2D::~VulkanRenderer2D()
    {
        if (device_.device() != nullptr)
        {
            vkDeviceWaitIdle(device_.device());
        }
    }

    void VulkanRenderer2D::begin_frame()
    {
    }

    void VulkanRenderer2D::end_frame()
    {
        auto dev = device_.device();

        auto in_flight = sync_.in_flight(current_frame_);
        if (dev.waitForFences(1, &in_flight, vk::True, std::numeric_limits<uint64>::max()) == vk::Result::eTimeout)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to wait for fence"};
        }
        dev.resetFences({in_flight});

        uint32 image_index = 0;
        auto result = dev.acquireNextImageKHR(swapchain_.handle(),
                                              std::numeric_limits<uint64>::max(),
                                              sync_.image_available(current_frame_),
                                              nullptr,
                                              &image_index);

        if (result == vk::Result::eErrorOutOfDateKHR)
        {
            recreate_swapchain();
            return;
        }

        if (result != vk::Result::eSuccess && result != vk::Result::eSuboptimalKHR)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to acquire swapchain image"};
        }

        auto cmd = command_pool_.buffer_at(current_frame_);
        cmd.reset();
        record_command_buffer(cmd, image_index);

        std::array wait_semaphores = {sync_.image_available(current_frame_)};
        std::array wait_stages = {
            static_cast<vk::PipelineStageFlags>(vk::PipelineStageFlagBits::eColorAttachmentOutput)};

        // Signal semaphore is now per-image
        const vk::Semaphore render_finished_semaphore = sync_.render_finished(image_index);
        std::array signal_semaphores = {render_finished_semaphore};

        const vk::SubmitInfo submit_info{wait_semaphores.size(),
                                         wait_semaphores.data(),
                                         wait_stages.data(),
                                         1,
                                         &cmd,
                                         signal_semaphores.size(),
                                         signal_semaphores.data()};

        if (device_.graphics_queue().submit(1, &submit_info, in_flight) != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to submit draw command buffer"};
        }

        std::array swapchains = {swapchain_.handle()};

        vk::PresentInfoKHR present_info{signal_semaphores.size(),
                                        signal_semaphores.data(),
                                        swapchains.size(),
                                        swapchains.data(),
                                        &image_index};

        result = device_.present_queue().presentKHR(&present_info);

        if (result == vk::Result::eErrorOutOfDateKHR || result == vk::Result::eSuboptimalKHR)
        {
            recreate_swapchain();
        }
        else if (result != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to present swapchain image"};
        }

        current_frame_ = (current_frame_ + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    void VulkanRenderer2D::draw_quad(Vector2 position, Vector2 size, Color color)
    {
        // Placeholder: real drawing will come once the pipeline is set up.
        (void)position;
        (void)size;
        (void)color;
    }

    vk::UniqueInstance VulkanRenderer2D::create_instance()
    {
        vk::ApplicationInfo app_info{"Retro Engine",
                                     vk::makeVersion(1, 0, 0),
                                     "Retro Engine",
                                     vk::makeVersion(1, 0, 0),
                                     vk::makeApiVersion(0, 1, 2, 0)};

        std::vector<const char *> enabled_layers;
#ifndef NDEBUG
        auto available_layers = vk::enumerateInstanceLayerProperties();
        const bool has_validation =
            std::ranges::any_of(available_layers,
                                [](const vk::LayerProperties &lp)
                                { return std::string_view{lp.layerName} == "VK_LAYER_KHRONOS_validation"; });

        if (has_validation)
        {
            enabled_layers.push_back("VK_LAYER_KHRONOS_validation");
        }
        else
        {
            std::cerr << "WARNING: Vulkan validation layers requested, but not available!\n";
        }
#endif

        const auto extensions = get_required_instance_extensions();

        vk::InstanceCreateInfo create_info{{},
                                           &app_info,
                                           static_cast<uint32>(enabled_layers.size()),
                                           enabled_layers.data(),
                                           static_cast<uint32>(extensions.size()),
                                           extensions.data()};

        return vk::createInstanceUnique(create_info);
    }

    std::span<const char *const> VulkanRenderer2D::get_required_instance_extensions()
    {
        // Ask SDL what Vulkan instance extensions are required for this window
        uint32 count = 0;
        auto *names = SDL_Vulkan_GetInstanceExtensions(&count);
        if (names == nullptr)
        {
            throw std::runtime_error("SDL_Vulkan_GetInstanceExtensions failed");
        }

        return std::span{names, count};
    }

    vk::UniqueRenderPass VulkanRenderer2D::create_render_pass(vk::Device device,
                                                              vk::Format color_format,
                                                              vk::SampleCountFlagBits samples)
    {
        vk::AttachmentDescription color_attachment{{},
                                                   color_format,
                                                   samples,
                                                   vk::AttachmentLoadOp::eClear,
                                                   vk::AttachmentStoreOp::eStore,
                                                   vk::AttachmentLoadOp::eDontCare,
                                                   vk::AttachmentStoreOp::eDontCare,
                                                   vk::ImageLayout::eUndefined,
                                                   vk::ImageLayout::ePresentSrcKHR};

        vk::AttachmentReference color_ref{0, vk::ImageLayout::eColorAttachmentOptimal};

        vk::SubpassDescription subpass{{}, vk::PipelineBindPoint::eGraphics, 0, nullptr, 1, &color_ref};

        vk::SubpassDependency dependency{vk::SubpassExternal,
                                         0,
                                         vk::PipelineStageFlagBits::eColorAttachmentOutput,
                                         vk::PipelineStageFlagBits::eColorAttachmentOutput,
                                         vk::AccessFlagBits::eNone,
                                         vk::AccessFlagBits::eColorAttachmentWrite,
                                         vk::DependencyFlagBits::eByRegion};

        vk::RenderPassCreateInfo rp_info{{}, 1, &color_attachment, 1, &subpass, 1, &dependency};

        return device.createRenderPassUnique(rp_info);
    }

    std::vector<vk::UniqueFramebuffer> VulkanRenderer2D::create_framebuffers(vk::Device device,
                                                                             vk::RenderPass render_pass,
                                                                             const VulkanSwapchain &swapchain)
    {
        return swapchain.image_views() |
               std::views::transform(
                   [device, render_pass, &swapchain](const vk::UniqueImageView &image)
                   {
                       std::array attachments = {image.get()};

                       vk::FramebufferCreateInfo fb_info{{},
                                                         render_pass,
                                                         attachments.size(),
                                                         attachments.data(),
                                                         swapchain.extent().width,
                                                         swapchain.extent().height,
                                                         1};

                       return device.createFramebufferUnique(fb_info);
                   }) |
               std::ranges::to<std::vector>();
    }

    void VulkanRenderer2D::create_pipeline()
    {
        auto device = device_.device();

        // TODO: adjust shader paths as needed
        auto vert_module = create_shader_module(device, "shaders/fullscreen_quad.vert.spv");
        auto frag_module = create_shader_module(device, "shaders/solid_color.frag.spv");

        vk::PipelineShaderStageCreateInfo vert_stage{{}, vk::ShaderStageFlagBits::eVertex, vert_module.get(), "main"};

        vk::PipelineShaderStageCreateInfo frag_stage{{}, vk::ShaderStageFlagBits::eFragment, frag_module.get(), "main"};

        std::array shader_stages = {vert_stage, frag_stage};

        // No vertex buffers: all positions come from gl_VertexIndex
        vk::PipelineVertexInputStateCreateInfo vertex_input{};

        vk::PipelineInputAssemblyStateCreateInfo input_assembly{{}, vk::PrimitiveTopology::eTriangleList, vk::False};

        vk::Viewport viewport{0.0f,
                              0.0f,
                              static_cast<float>(swapchain_.extent().width),
                              static_cast<float>(swapchain_.extent().height),
                              0.0f,
                              1.0f};

        vk::Rect2D scissor{{0, 0}, swapchain_.extent()};

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

        // No descriptor sets, no push constants yet
        vk::PipelineLayoutCreateInfo pipeline_layout_info{};
        pipeline_layout_ = device.createPipelineLayoutUnique(pipeline_layout_info);

        vk::GraphicsPipelineCreateInfo pipeline_info{{},
                                                     static_cast<uint32_t>(shader_stages.size()),
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
                                                     pipeline_layout_.get(),
                                                     render_pass_.get(),
                                                     0};

        auto [result, pipeline] = device.createGraphicsPipelineUnique(nullptr, pipeline_info);
        if (result != vk::Result::eSuccess)
        {
            throw std::runtime_error{"VulkanRenderer2D: failed to create graphics pipeline"};
        }

        graphics_pipeline_ = std::move(pipeline);
    }

    vk::UniqueShaderModule VulkanRenderer2D::create_shader_module(const vk::Device device,
                                                                  const std::filesystem::path &path)
    {
        const auto bytes = read_binary_file(path);
        const auto *code = std::bit_cast<const uint32_t *>(bytes.data());

        if (bytes.size() % sizeof(uint32) != 0)
        {
            throw std::runtime_error{"SPIR-V file size is not a multiple of 4 bytes"};
        }

        const vk::ShaderModuleCreateInfo info{{}, bytes.size(), code};
        return device.createShaderModuleUnique(info);
    }

    void VulkanRenderer2D::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = viewport_->size();
        if (w == 0 || h == 0)
            return;

        device_.device().waitIdle();

        swapchain_ = VulkanSwapchain{SwapchainConfig{SwapchainConfig{
            .physical_device = device_.physical_device(),
            .device = device_.device(),
            .surface = surface_.get(),
            .graphics_family = device_.graphics_family_index(),
            .present_family = device_.present_family_index(),
            .width = w,
            .height = h,
        }}};
        render_pass_ = create_render_pass(device_.device(), swapchain_.format(), vk::SampleCountFlagBits::e1);
        framebuffers_ = create_framebuffers(device_.device(), render_pass_.get(), swapchain_);
        create_pipeline();
    }

    void VulkanRenderer2D::record_command_buffer(const vk::CommandBuffer cmd, const uint32 image_index)
    {
        constexpr vk::CommandBufferBeginInfo begin_info{};

        cmd.begin(begin_info);
        vk::ClearValue clear{{0.1f, 0.1f, 0.2f, 1.0f}};

        const vk::RenderPassBeginInfo rp_info{render_pass_.get(),
                                              framebuffers_.at(image_index).get(),
                                              vk::Rect2D{vk::Offset2D{0, 0}, swapchain_.extent()},
                                              1,
                                              &clear};

        cmd.beginRenderPass(rp_info, vk::SubpassContents::eInline);
        cmd.bindPipeline(vk::PipelineBindPoint::eGraphics, graphics_pipeline_.get());
        cmd.draw(6, 1, 0, 0);
        cmd.endRenderPass();
        cmd.end();
    }
} // namespace retro
