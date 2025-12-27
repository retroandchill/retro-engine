//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    void VulkanRenderPass::recreate(const RenderPassConfig &cfg)
    {
        reset();
        create(cfg);
    }

    void VulkanRenderPass::reset() noexcept
    {
        if (device_ != VK_NULL_HANDLE && render_pass_ != VK_NULL_HANDLE)
        {
            vkDestroyRenderPass(device_, render_pass_, nullptr);
            render_pass_ = VK_NULL_HANDLE;
        }
        device_ = VK_NULL_HANDLE;
    }

    void VulkanRenderPass::create(const RenderPassConfig &cfg)
    {
        if (cfg.device == VK_NULL_HANDLE || cfg.color_format == VK_FORMAT_UNDEFINED)
        {
            throw std::runtime_error{"VulkanRenderPass: invalid config"};
        }

        device_ = cfg.device;

        VkAttachmentDescription color_attachment{};
        color_attachment.format = cfg.color_format;
        color_attachment.samples = cfg.samples;
        color_attachment.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
        color_attachment.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
        color_attachment.stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
        color_attachment.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
        color_attachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
        color_attachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

        VkAttachmentReference color_ref{};
        color_ref.attachment = 0;
        color_ref.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

        VkSubpassDescription subpass{};
        subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
        subpass.colorAttachmentCount = 1;
        subpass.pColorAttachments = &color_ref;

        VkRenderPassCreateInfo rp_info{};
        rp_info.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
        rp_info.attachmentCount = 1;
        rp_info.pAttachments = &color_attachment;
        rp_info.subpassCount = 1;
        rp_info.pSubpasses = &subpass;

        if (vkCreateRenderPass(device_, &rp_info, nullptr, &render_pass_) != VK_SUCCESS)
        {
            throw std::runtime_error{"VulkanRenderPass: failed to create render pass"};
        }
    }
} // namespace retro
