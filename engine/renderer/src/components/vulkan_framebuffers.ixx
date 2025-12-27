//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_framebuffers;

import std;

namespace retro
{
    export struct FramebufferConfig
    {
        VkDevice device = VK_NULL_HANDLE;
        VkRenderPass render_pass = VK_NULL_HANDLE;
        VkExtent2D extent{};
        const std::vector<VkImageView> *image_views = nullptr;
    };

    export class RETRO_API VulkanFramebuffers
    {
      public:
        explicit inline VulkanFramebuffers(const FramebufferConfig &cfg)
        {
            create(cfg);
        }

        inline ~VulkanFramebuffers() noexcept
        {
            reset();
        }

        VulkanFramebuffers(const VulkanFramebuffers &) = delete;
        VulkanFramebuffers &operator=(const VulkanFramebuffers &) = delete;
        VulkanFramebuffers(VulkanFramebuffers &&) = delete;
        VulkanFramebuffers &operator=(VulkanFramebuffers &&) = delete;

        void recreate(const FramebufferConfig &cfg);
        void reset() noexcept;

        [[nodiscard]] inline const std::vector<VkFramebuffer> &framebuffers() const noexcept
        {
            return framebuffers_;
        }

        [[nodiscard]] inline VkFramebuffer framebuffer_at(size_t index) const noexcept
        {
            return framebuffers_[index];
        }

      private:
        void create(const FramebufferConfig &cfg);

        VkDevice device_{VK_NULL_HANDLE};
        VkRenderPass render_pass_{VK_NULL_HANDLE};
        VkExtent2D extent_{};
        std::vector<VkFramebuffer> framebuffers_;
    };
} // namespace retro