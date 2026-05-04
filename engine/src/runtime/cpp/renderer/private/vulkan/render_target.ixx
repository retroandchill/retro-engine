/**
 * @file render_target.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.render_target;

import vulkan;
import retro.runtime.rendering.render_target;
import retro.core.math.vector;
import retro.runtime.rendering.texture;
import retro.platform.window;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.pipeline;
import retro.renderer.vulkan.components.pipeline;

namespace retro
{
    struct VulkanImageResources
    {
        vk::Image color_image;
        vk::UniqueImageView color_image_view;
        vk::UniqueImage depth_image;
        vk::UniqueDeviceMemory depth_image_memory;
        vk::UniqueImageView depth_image_view;
        vk::UniqueSemaphore render_finished;
        vk::UniqueFramebuffer framebuffer;
    };

    export class VulkanRenderTarget
    {
      public:
        virtual ~VulkanRenderTarget() = default;

        virtual void acquire_next_image(vk::Semaphore semaphore, std::uint32_t image_index) = 0;

        virtual void present(std::uint32_t image_index, std::span<vk::Semaphore> signal_semaphores) = 0;
    };

    export class VulkanWindowRenderTarget final : public WindowRenderTarget, public VulkanRenderTarget
    {
      public:
        explicit VulkanWindowRenderTarget(std::uint64_t id,
                                          std::unique_ptr<Window> window,
                                          vk::UniqueSurfaceKHR surface,
                                          VulkanDevice &device,
                                          VulkanPipelineManager &pipeline_manager);

        [[nodiscard]] inline Vector2u size() const noexcept override
        {
            return window_->size();
        }

        [[nodiscard]] inline TextureFormat format() const noexcept override
        {
            return TextureFormat::rgba8;
        }

        [[nodiscard]] Window &window() const noexcept override
        {
            return *window_;
        }

        void acquire_next_image(vk::Semaphore semaphore, std::uint32_t image_index) override;

        void present(std::uint32_t image_index, std::span<vk::Semaphore> signal_semaphores) override;

      private:
        void recreate_swapchain();

        void create_swapchain(std::uint32_t width, std::uint32_t height);

        std::unique_ptr<Window> window_;
        vk::UniqueSurfaceKHR surface_;
        VulkanDevice &device_;

        vk::UniqueSwapchainKHR swapchain_;
        vk::UniqueRenderPass render_pass_;
        vk::Format format_{vk::Format::eUndefined};
        vk::Extent2D extent_{};
        std::vector<VulkanImageResources> image_resources_;

        VulkanPipelineManager &pipeline_manager_;
    };
} // namespace retro
