//
// Created by fcors on 12/26/2025.
//
module;

#include <vulkan/vulkan.h>

module retro.renderer;

namespace retro
{
    void VulkanFramebuffers::recreate(const FramebufferConfig &cfg)
    {
        reset();
        create(cfg);
    }

    void VulkanFramebuffers::reset() noexcept
    {
        if (device_ != VK_NULL_HANDLE)
        {
            for (auto fb : framebuffers_)
            {
                vkDestroyFramebuffer(device_, fb, nullptr);
            }
        }
        framebuffers_.clear();
        device_ = VK_NULL_HANDLE;
        render_pass_ = VK_NULL_HANDLE;
        extent_ = {};
    }

    void VulkanFramebuffers::create(const FramebufferConfig &cfg)
    {
        if (!cfg.device || !cfg.render_pass || !cfg.image_views || cfg.image_views->empty())
        {
            throw std::runtime_error{"VulkanFramebuffers: invalid config"};
        }

        device_ = cfg.device;
        render_pass_ = cfg.render_pass;
        extent_ = cfg.extent;

        framebuffers_.resize(cfg.image_views->size());

        for (size_t i = 0; i < cfg.image_views->size(); ++i)
        {
            VkImageView attachments[] = {(*cfg.image_views)[i]};

            VkFramebufferCreateInfo fb_info{};
            fb_info.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
            fb_info.renderPass = render_pass_;
            fb_info.attachmentCount = 1;
            fb_info.pAttachments = attachments;
            fb_info.width = extent_.width;
            fb_info.height = extent_.height;
            fb_info.layers = 1;

            if (vkCreateFramebuffer(device_, &fb_info, nullptr, &framebuffers_[i]) != VK_SUCCESS)
            {
                throw std::runtime_error{"VulkanFramebuffers: failed to create framebuffer"};
            }
        }
    }
} // namespace retro
