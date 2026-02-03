/**
 * @file renderer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.renderer;

import retro.runtime.rendering.texture_render_data;
import retro.runtime.rendering.renderer2d;
import :components;
import :pipeline;
import retro.core.di;
import retro.platform.window;
import vulkan_hpp;
import std;

namespace retro
{

    export struct TransientAllocation
    {
        vk::Buffer buffer;
        void *mapped_data;
        size_t offset;
    };

    class RETRO_API VulkanBufferManager
    {
        explicit VulkanBufferManager(const VulkanDevice &device, std::size_t pool_size);

      public:
        constexpr static std::size_t DEFAULT_POOL_SIZE = 1024 * 1024 * 10;

        static void initialize(const VulkanDevice &device, std::size_t pool_size = DEFAULT_POOL_SIZE);

        static void shutdown();

        static VulkanBufferManager &instance();

        TransientAllocation allocate_transient(std::size_t size, vk::BufferUsageFlags usage);

        void reset();

      private:
        vk::PhysicalDevice physical_device_;
        vk::Device device_;
        vk::UniqueBuffer buffer_;
        vk::UniqueDeviceMemory memory_;
        void *mapped_ptr_ = nullptr;
        std::size_t pool_size_{DEFAULT_POOL_SIZE};
        std::size_t current_offset_ = 0;

        static std::unique_ptr<VulkanBufferManager> instance_;
    };

    class VulkanBufferManagerScope
    {
      public:
        explicit inline VulkanBufferManagerScope(const VulkanDevice &device,
                                                 const std::size_t pool_size = VulkanBufferManager::DEFAULT_POOL_SIZE)
        {
            VulkanBufferManager::initialize(device, pool_size);
        }

        VulkanBufferManagerScope(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope(VulkanBufferManagerScope &&) noexcept = delete;

        inline ~VulkanBufferManagerScope()
        {
            VulkanBufferManager::shutdown();
        }

        VulkanBufferManagerScope &operator=(const VulkanBufferManagerScope &) = delete;
        VulkanBufferManagerScope &operator=(VulkanBufferManagerScope &&) noexcept = delete;
    };

    class VulkanTextureRenderData final : public TextureRenderData
    {
      public:
        inline VulkanTextureRenderData(vk::UniqueImage image,
                                       vk::UniqueDeviceMemory memory,
                                       vk::UniqueImageView view,
                                       vk::Sampler sampler,
                                       std::int32_t width,
                                       std::int32_t height) noexcept
            : TextureRenderData{width, height}, memory_{std::move(memory)}, image_{std::move(image)},
              view_{std::move(view)}, sampler_{sampler}
        {
        }

        [[nodiscard]] inline vk::ImageView view() const noexcept
        {
            return view_.get();
        }

        [[nodiscard]] inline vk::Sampler sampler() const noexcept
        {
            return sampler_;
        }

      private:
        vk::UniqueImage image_;
        vk::UniqueDeviceMemory memory_;
        vk::UniqueImageView view_;
        vk::Sampler sampler_;
    };

    export class RETRO_API VulkanRenderer2D final : public Renderer2D
    {
      public:
        using Dependencies = TypeList<Window>;

        explicit VulkanRenderer2D(std::shared_ptr<Window> viewport);

        VulkanRenderer2D(const VulkanRenderer2D &) = delete;
        VulkanRenderer2D(VulkanRenderer2D &&) noexcept = delete;

        VulkanRenderer2D &operator=(VulkanRenderer2D &&) = delete;
        VulkanRenderer2D &operator=(const VulkanRenderer2D &) = delete;

        void wait_idle() override;

        void begin_frame() override;

        void end_frame() override;

        [[nodiscard]] Vector2u viewport_size() const override;

        void add_new_render_pipeline(std::type_index type, std::shared_ptr<RenderPipeline> pipeline) override;

        void remove_render_pipeline(std::type_index type) override;

        std::unique_ptr<TextureRenderData> upload_texture(const ImageData &image_data) override;

      private:
        static vk::UniqueInstance create_instance(const Window &viewport);
        static vk::UniqueSurfaceKHR create_surface(const Window &viewport, vk::Instance instance);
        static std::span<const char *const> get_required_instance_extensions(const Window &viewport);
        static vk::UniqueRenderPass create_render_pass(vk::Device device,
                                                       vk::Format color_format,
                                                       vk::SampleCountFlagBits samples);
        static std::vector<vk::UniqueFramebuffer> create_framebuffers(vk::Device device,
                                                                      vk::RenderPass render_pass,
                                                                      const VulkanSwapchain &swapchain);
        [[nodiscard]] vk::UniqueSampler create_linear_sampler() const;

        void recreate_swapchain();
        void record_command_buffer(vk::CommandBuffer cmd, std::uint32_t image_index);

        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands() const;
        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

        static void transition_image_layout(vk::CommandBuffer cmd,
                                            vk::Image image,
                                            vk::ImageLayout old_layout,
                                            vk::ImageLayout new_layout);

      private:
        std::shared_ptr<Window> viewport_;

        vk::UniqueInstance instance_;
        vk::UniqueSurfaceKHR surface_;
        VulkanDevice device_;
        VulkanBufferManagerScope buffer_manager_;
        VulkanSwapchain swapchain_;
        vk::UniqueRenderPass render_pass_;
        std::vector<vk::UniqueFramebuffer> framebuffers_;
        VulkanCommandPool command_pool_;
        VulkanSyncObjects sync_;
        vk::UniqueSampler linear_sampler_;
        VulkanPipelineManager pipeline_manager_;

        std::uint32_t current_frame_ = 0;
        std::uint32_t image_index_ = 0;

        static constexpr std::uint32_t MAX_FRAMES_IN_FLIGHT = 2;
    };

    export inline void add_rendering_services(ServiceCollection &services, std::shared_ptr<Window> viewport)
    {
        services.add_singleton(std::move(viewport));
        services.add_singleton<Renderer2D, VulkanRenderer2D>();
    }
} // namespace retro
