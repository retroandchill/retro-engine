/**
 * @file swapchain.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#if __JETBRAINS_IDE__
#include <vulkan/vulkan.hpp>
#endif

module retro.renderer.vulkan.components.swapchain;

import retro.renderer.vulkan.components.buffer_manager;

namespace retro
{
    namespace
    {
        vk::UniqueRenderPass create_render_pass(const vk::Device device,
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

            return device.createRenderPassUnique(rp_info);
        }
    } // namespace

    VulkanSwapchain::VulkanSwapchain(const SwapchainConfig &config)
    {
        const auto capabilities = config.physical_device.getSurfaceCapabilitiesKHR(config.surface);

        const vk::Extent2D desired_extent{config.width, config.height};

        vk::Extent2D actual_extent{std::clamp<std::uint32_t>(desired_extent.width,
                                                             capabilities.minImageExtent.width,
                                                             capabilities.maxImageExtent.width),
                                   std::clamp<std::uint32_t>(desired_extent.height,
                                                             capabilities.minImageExtent.height,
                                                             capabilities.maxImageExtent.height)};

        const auto formats = config.physical_device.getSurfaceFormatsKHR(config.surface);
        if (formats.empty())
        {
            throw std::runtime_error{"VulkanSwapchain: no surface formats"};
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

        if (const auto present_modes = config.physical_device.getSurfacePresentModesKHR(config.surface);
            present_modes.empty())
        {
            throw std::runtime_error{"VulkanSwapchain: no present modes"};
        }

        auto chosen_present_mode = vk::PresentModeKHR::eFifo; // always available

        std::uint32_t image_count = capabilities.minImageCount + 1;
        if (capabilities.maxImageCount > 0 && image_count > capabilities.maxImageCount)
        {
            image_count = capabilities.maxImageCount;
        }

        vk::SwapchainCreateInfoKHR ci{
            .surface = config.surface,
            .minImageCount = image_count,
            .imageFormat = chosen_format.format,
            .imageColorSpace = chosen_format.colorSpace,
            .imageExtent = actual_extent,
            .imageArrayLayers = 1,
            .imageUsage = vk::ImageUsageFlagBits::eColorAttachment,
        };

        // ReSharper disable once CppDFAUnreadVariable
        // ReSharper disable once CppDFAUnusedValue
        std::array queue_family_indices = {config.graphics_family, config.present_family};

        if (config.graphics_family != config.present_family)
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
        ci.oldSwapchain = config.old_swapchain;

        swapchain_ = config.device.createSwapchainKHRUnique(ci);

        format_ = chosen_format.format;
        extent_ = actual_extent;

        render_pass_ = create_render_pass(config.device, format_, vk::SampleCountFlagBits::e1);
        image_resources_ =
            config.device.getSwapchainImagesKHR(swapchain_.get()) |
            std::views::transform(
                [this, &config, &actual_extent](const vk::Image image)
                {
                    auto depth_image = config.device.createImageUnique(
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

                    // ReSharper disable once CppDFAUnreadVariable
                    // ReSharper disable once CppDFAUnusedValue
                    auto mem_req = config.device.getImageMemoryRequirements(depth_image.get());
                    vk::MemoryAllocateInfo alloc_info{.allocationSize = mem_req.size,
                                                      .memoryTypeIndex =
                                                          find_memory_type(config.physical_device,
                                                                           mem_req.memoryTypeBits,
                                                                           vk::MemoryPropertyFlagBits::eDeviceLocal)};

                    auto depth_memory = config.device.allocateMemoryUnique(alloc_info);
                    config.device.bindImageMemory(depth_image.get(), depth_memory.get(), 0);

                    auto color_view = config.device.createImageViewUnique(vk::ImageViewCreateInfo{
                        .image = image,
                        .viewType = vk::ImageViewType::e2D,
                        .format = format_,
                        .components = vk::ComponentMapping{vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity,
                                                           vk::ComponentSwizzle::eIdentity},
                        .subresourceRange = vk::ImageSubresourceRange{vk::ImageAspectFlagBits::eColor, 0, 1, 0, 1},
                    });

                    auto depth_view = config.device.createImageViewUnique(vk::ImageViewCreateInfo{
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
                                                .render_finished =
                                                    config.device.createSemaphoreUnique(vk::SemaphoreCreateInfo{}),
                                                .framebuffer = config.device.createFramebufferUnique(fb_info)};
                }) |
            std::ranges::to<std::vector>();
    }
} // namespace retro
