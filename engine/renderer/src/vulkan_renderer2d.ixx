//
// Created by fcors on 12/26/2025.
//
module;

#include "retro/core/exports.h"

#include <vulkan/vulkan.hpp>

export module retro.renderer:vulkan_renderer2d;

import retro.runtime;
import :vulkan_viewport;
import :components;

namespace retro
{
    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        explicit VulkanRenderer2D(std::shared_ptr<VulkanViewport> viewport);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        ~VulkanRenderer2D() override;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        void begin_frame() override;

        void end_frame() override;

        void draw_quad(Vector2 position, Vector2 size, Color color) override;

      private:
        static vk::InstanceCreateInfo get_instance_create_info();
        static std::span<const char *const> get_required_instance_extensions();
        static vk::UniqueRenderPass create_render_pass(vk::Device device,
                                                       vk::Format color_format,
                                                       vk::SampleCountFlagBits samples);
        static std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
                                                                      vk::RenderPass render_pass,
                                                                      const VulkanSwapchain &swapchain);

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, uint32 image_index);

        std::shared_ptr<VulkanViewport> viewport_;

        vk::UniqueInstance instance_;
        vk::UniqueSurfaceKHR surface_;
        VulkanDevice device_;
        VulkanSwapchain swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        VulkanCommandPool command_pool_;
        VulkanSyncObjects sync_;

        uint32 current_frame_ = 0;

        static constexpr uint32 MAX_FRAMES_IN_FLIGHT = 2;
    };
} // namespace retro