//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

export module retro.renderer:vulkan_renderer2d;

import retro.runtime;
import :window;
import :components;

namespace retro
{
    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        explicit VulkanRenderer2D(Window window);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        ~VulkanRenderer2D() override;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        void begin_frame() override;

        void end_frame() override;

        void draw_quad(Vector2 position, Vector2 size, Color color) override;

      private:
        static VulkanInstance get_instance_create_info();
        static std::span<const char *const> get_required_instance_extensions();

        void recreate_swapchain();
        void record_command_buffer(VkCommandBuffer cmd, uint32 image_index);

        Window window_;

        VulkanInstance instance_;
        VulkanSurface surface_;
        VulkanDevice device_;
        VulkanSwapchain swapchain_;
        VulkanRenderPass render_pass_;
        VulkanFramebuffers framebuffers_;
        VulkanCommandPool command_pool_;
        VulkanSyncObjects sync_;

        uint32 current_frame_ = 0;

        static constexpr uint32 MAX_FRAMES_IN_FLIGHT = 2;
    };
} // namespace retro