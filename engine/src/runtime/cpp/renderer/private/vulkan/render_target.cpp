/**
 * @file render_target.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.renderer.vulkan.render_target;
import retro.runtime.exceptions;

namespace retro
{
    namespace
    {
        vk::UniqueRenderPass create_render_pass(VulkanDevice &device,
                                                const vk::Format color_format,
                                                const vk::SampleCountFlagBits samples)
        {
            const vk::AttachmentDescription color_attachment{.format = color_format,
                                                             .samples = samples,
                                                             .loadOp = vk::AttachmentLoadOp::eClear,
                                                             .storeOp = vk::AttachmentStoreOp::eStore,
                                                             .stencilLoadOp = vk::AttachmentLoadOp::eDontCare,
                                                             .stencilStoreOp = vk::AttachmentStoreOp::eDontCare,
                                                             .initialLayout = vk::ImageLayout::eUndefined,
                                                             .finalLayout = vk::ImageLayout::ePresentSrcKHR};

            const vk::AttachmentDescription depth_attachment{.format = vk::Format::eD32Sfloat,
                                                             .samples = samples,
                                                             .loadOp = vk::AttachmentLoadOp::eClear,
                                                             .storeOp = vk::AttachmentStoreOp::eDontCare,
                                                             .stencilLoadOp = vk::AttachmentLoadOp::eDontCare,
                                                             .stencilStoreOp = vk::AttachmentStoreOp::eDontCare,
                                                             .initialLayout = vk::ImageLayout::eUndefined,
                                                             .finalLayout =
                                                                 vk::ImageLayout::eDepthStencilAttachmentOptimal};

            std::array attachments = {color_attachment, depth_attachment};

            vk::AttachmentReference color_ref{.attachment = 0, .layout = vk::ImageLayout::eColorAttachmentOptimal};
            vk::AttachmentReference depth_ref{.attachment = 1,
                                              .layout = vk::ImageLayout::eDepthStencilAttachmentOptimal};

            vk::SubpassDescription subpass{.pipelineBindPoint = vk::PipelineBindPoint::eGraphics,
                                           .colorAttachmentCount = 1,
                                           .pColorAttachments = &color_ref,
                                           .pDepthStencilAttachment = &depth_ref};

            vk::SubpassDependency dependency{.srcSubpass = vk::SubpassExternal,
                                             .srcStageMask = vk::PipelineStageFlagBits::eColorAttachmentOutput |
                                                             vk::PipelineStageFlagBits::eEarlyFragmentTests,
                                             .dstStageMask = vk::PipelineStageFlagBits::eColorAttachmentOutput |
                                                             vk::PipelineStageFlagBits::eEarlyFragmentTests,
                                             .srcAccessMask = vk::AccessFlagBits::eNone,
                                             .dstAccessMask = vk::AccessFlagBits::eColorAttachmentWrite |
                                                              vk::AccessFlagBits::eDepthStencilAttachmentWrite,
                                             .dependencyFlags = vk::DependencyFlagBits::eByRegion};

            vk::RenderPassCreateInfo rp_info{.attachmentCount = attachments.size(),
                                             .pAttachments = attachments.data(),
                                             .subpassCount = 1,
                                             .pSubpasses = &subpass,
                                             .dependencyCount = 1,
                                             .pDependencies = &dependency};

            return device.create_render_pass(rp_info);
        }
    } // namespace

    VulkanWindowRenderTarget::VulkanWindowRenderTarget(const std::uint64_t id,
                                                       std::unique_ptr<Window> window,
                                                       vk::UniqueSurfaceKHR surface,
                                                       VulkanDevice &device,
                                                       VulkanPipelineManager &pipeline_manager)
        : WindowRenderTarget{id}, window_{std::move(window)}, surface_{std::move(surface)}, device_{device},
          pipeline_manager_{pipeline_manager}
    {
        auto [width, height] = window_->size();
        create_swapchain(width, height);
    }
    void VulkanWindowRenderTarget::acquire_next_image(vk::Semaphore semaphore, std::uint32_t image_index)
    {
        const auto result = device_.acquire_next_image(swapchain_.get(),
                                                       std::numeric_limits<std::uint64_t>::max(),
                                                       semaphore,
                                                       nullptr,
                                                       image_index);

        if (result == vk::Result::eErrorOutOfDateKHR)
        {
            recreate_swapchain();
            return;
        }

        if (result != vk::Result::eSuccess && result != vk::Result::eSuboptimalKHR)
        {
            throw GraphicsException{"VulkanRenderer2D: failed to acquire swapchain image"};
        }
    }
    void VulkanWindowRenderTarget::present(std::uint32_t image_index, std::span<vk::Semaphore> signal_semaphores)
    {
        std::array swapchains = {swapchain_.get()};

        vk::PresentInfoKHR present_info{.waitSemaphoreCount = static_cast<std::uint32_t>(signal_semaphores.size()),
                                        .pWaitSemaphores = signal_semaphores.data(),
                                        .swapchainCount = swapchains.size(),
                                        .pSwapchains = swapchains.data(),
                                        .pImageIndices = &image_index};

        device_.submit_to_present_queue(
            [this, &present_info](vk::Queue present_queue)
            {
                if (const auto result = present_queue.presentKHR(&present_info);
                    result == vk::Result::eErrorOutOfDateKHR || result == vk::Result::eSuboptimalKHR)
                {
                    recreate_swapchain();
                }
                else if (result != vk::Result::eSuccess)
                {
                    throw GraphicsException{"VulkanRenderer2D: failed to present swapchain image"};
                }
            });
    }

    void VulkanWindowRenderTarget::recreate_swapchain()
    {
        // Query new size from window_
        const auto [w, h] = window_->size();
        if (w == 0 || h == 0)
            return;

        device_.wait_idle();
        create_swapchain(w, h);
        pipeline_manager_.recreate_pipelines(extent_, render_pass_.get());
    }

    void VulkanWindowRenderTarget::create_swapchain(std::uint32_t width, std::uint32_t height)
    {
        const auto capabilities = device_.get_surface_capabilities(surface_.get());

        const vk::Extent2D desired_extent{width, height};

        vk::Extent2D actual_extent{std::clamp<std::uint32_t>(desired_extent.width,
                                                             capabilities.minImageExtent.width,
                                                             capabilities.maxImageExtent.width),
                                   std::clamp<std::uint32_t>(desired_extent.height,
                                                             capabilities.minImageExtent.height,
                                                             capabilities.maxImageExtent.height)};

        const auto formats = device_.get_surface_formats(surface_.get());
        if (formats.empty())
        {
            throw GraphicsException{"VulkanSwapchain: no surface formats"};
        }

        auto chosen_format = formats[0];
        for (const auto &f : formats)
        {
            if (f.format == vk::Format::eB8G8R8A8Srgb && f.colorSpace == vk::ColorSpaceKHR::eSrgbNonlinear)
            {
                chosen_format = f;
                break;
            }
        }

        if (const auto present_modes = device_.get_surface_preset_modes(surface_.get()); present_modes.empty())
        {
            throw GraphicsException{"VulkanSwapchain: no present modes"};
        }

        auto chosen_present_mode = vk::PresentModeKHR::eFifo; // always available

        std::uint32_t image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        vk::SwapchainCreateInfoKHR ci{
            .surface = surface_.get(),
            .minImageCount = image_count,
            .imageFormat = chosen_format.format,
            .imageColorSpace = chosen_format.colorSpace,
            .imageExtent = actual_extent,
            .imageArrayLayers = 1,
            .imageUsage = vk::ImageUsageFlagBits::eColorAttachment,
        };

        const auto graphics_family = device_.graphics_family();
        const auto present_family = device_.present_family();
        const std::array queue_family_indices = {graphics_family, present_family};

        if (graphics_family != present_family)
        {
            ci.imageSharingMode = vk::SharingMode::eConcurrent;
            ci.queueFamilyIndexCount = queue_family_indices.size();
            ci.pQueueFamilyIndices = queue_family_indices.data();
        }
        else
        {
            ci.imageSharingMode = vk::SharingMode::eExclusive;
        }

        ci.preTransform = capabilities.currentTransform;
        ci.compositeAlpha = vk::CompositeAlphaFlagBitsKHR::eOpaque;
        ci.presentMode = chosen_present_mode;
        ci.clipped = vk::True;
        ci.oldSwapchain = swapchain_.get();

        swapchain_ = device_.create_swapchain(ci);

        format_ = chosen_format.format;
        extent_ = actual_extent;

        render_pass_ = create_render_pass(device_, format_, vk::SampleCountFlagBits::e1);
        image_resources_ =
            device_.get_swapchain_images(swapchain_.get()) |
            std::views::transform(
                [this, &actual_extent](const vk::Image image)
                {
                    auto depth_image = device_.create_image(
                        vk::ImageCreateInfo{.imageType = vk::ImageType::e2D,
                                            .format = vk::Format::eD32Sfloat,
                                            .extent = {actual_extent.width, actual_extent.height, 1},
                                            .mipLevels = 1,
                                            .arrayLayers = 1,
                                            .samples = vk::SampleCountFlagBits::e1,
                                            .tiling = vk::ImageTiling::eOptimal,
                                            .usage = vk::ImageUsageFlagBits::eDepthStencilAttachment,
                                            .sharingMode = vk::SharingMode::eExclusive,
                                            .initialLayout = vk::ImageLayout::eUndefined});

                    auto depth_memory = device_.allocate_image_memory(depth_image.get());

                    auto color_view = device_.create_image_view(vk::ImageViewCreateInfo{
                        .image = image,
                        .viewType = vk::ImageViewType::e2D,
                        .format = format_,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
                    });

                    auto depth_view = device_.create_image_view(vk::ImageViewCreateInfo{
                        .image = depth_image.get(),
                        .viewType = vk::ImageViewType::e2D,
                        .format = vk::Format::eD32Sfloat,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eDepth, 0, 1, 0, 1},
                    });

                    std::array attachments = {color_view.get(), depth_view.get()};

                    const vk::FramebufferCreateInfo fb_info{.renderPass = render_pass_.get(),
                                                            .attachmentCount = attachments.size(),
                                                            .pAttachments = attachments.data(),
                                                            .width = extent_.width,
                                                            .height = extent_.height,
                                                            .layers = 1};

                    return VulkanImageResources{.color_image = image,
                                                .color_image_view = std::move(color_view),
                                                .depth_image = std::move(depth_image),
                                                .depth_image_memory = std::move(depth_memory),
                                                .depth_image_view = std::move(depth_view),
                                                .render_finished = device_.create_semaphore(),
                                                .framebuffer = device_.create_framebuffer(fb_info)};
                }) |
            std::ranges::to<std::vector>();
    }
} // namespace retro
