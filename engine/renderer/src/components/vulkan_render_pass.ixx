//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.h>

export module retro.renderer:components.vulkan_render_pass;

namespace retro
{
    export struct RenderPassConfig
    {
        VkDevice device = VK_NULL_HANDLE;
        VkFormat color_format = VK_FORMAT_UNDEFINED;
        VkSampleCountFlagBits samples = VK_SAMPLE_COUNT_1_BIT;
    };

    export class RETRO_API VulkanRenderPass
    {
      public:
        explicit inline VulkanRenderPass(const RenderPassConfig &cfg)
        {
            create(cfg);
        }

        inline ~VulkanRenderPass() noexcept
        {
            reset();
        }

        VulkanRenderPass(const VulkanRenderPass &) = delete;
        VulkanRenderPass &operator=(const VulkanRenderPass &) = delete;
        VulkanRenderPass(VulkanRenderPass &&) = delete;
        VulkanRenderPass &operator=(VulkanRenderPass &&) = delete;

        void recreate(const RenderPassConfig &cfg);
        void reset() noexcept;

        [[nodiscard]] inline VkRenderPass handle() const noexcept
        {
            return render_pass_;
        }

      private:
        void create(const RenderPassConfig &cfg);

        VkDevice device_{VK_NULL_HANDLE};
        VkRenderPass render_pass_{VK_NULL_HANDLE};
    };
} // namespace retro