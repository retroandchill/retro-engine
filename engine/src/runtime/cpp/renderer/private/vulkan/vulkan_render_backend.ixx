/**
 * @file vulkan_render_backend.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.vulkan_render_backend;

import vulkan;
import retro.runtime.rendering.render_backend;
import retro.runtime.rendering.renderer2d;
import retro.platform.window;
import retro.core.memory.ref_counted_ptr;
import retro.renderer.vulkan.components.instance;
import retro.renderer.vulkan.components.device;
import retro.renderer.vulkan.components.buffer_manager;
import retro.platform.backend;
import retro.runtime.rendering.texture;
import retro.core.async.task;

namespace retro
{
    export class VulkanTexture;

    export class VulkanRenderBackend final : public RenderBackend
    {
      public:
        explicit VulkanRenderBackend(PlatformBackend &platform_backend);

        VulkanRenderBackend(const VulkanRenderBackend &) = delete;
        VulkanRenderBackend(VulkanRenderBackend &&) = delete;

        ~VulkanRenderBackend() override;

        VulkanRenderBackend &operator=(const VulkanRenderBackend &) = delete;
        VulkanRenderBackend &operator=(VulkanRenderBackend &&) = delete;

        std::shared_ptr<Renderer2D> create_renderer(std::shared_ptr<Window> window) override;

        Task<RefCountPtr<Texture>> upload_texture(std::span<const std::byte> bytes,
                                                  std::int32_t width,
                                                  std::int32_t height,
                                                  TextureFormat format,
                                                  TextureFilter filtering) override;

      private:
        struct TextureUploadPayload
        {
            VulkanStagingBuffer staging_buffer;
            vk::UniqueImage image;
            vk::UniqueDeviceMemory image_memory;
            std::int32_t width{};
            std::int32_t height{};
            TextureFormat format{};
            TextureFilter filtering{};
            vk::Format image_format{};
            TaskCompletionSource<RefCountPtr<Texture>> promise;

            TextureUploadPayload(VulkanStagingBuffer staging_buffer,
                                 std::int32_t width,
                                 std::int32_t height,
                                 TextureFormat format,
                                 TextureFilter filtering,
                                 vk::Format image_format) noexcept
                : staging_buffer(std::move(staging_buffer)), width(width), height(height), format(format),
                  filtering(filtering), image_format(image_format)
            {
            }
        };

        void run_transfer_thread();

        RefCountPtr<Texture> upload_texture_impl(TextureUploadPayload &payload);

        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands(vk::CommandPool command_pool) const;
        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

        static void transition_image_layout(vk::CommandBuffer cmd,
                                            vk::Image image,
                                            vk::ImageLayout old_layout,
                                            vk::ImageLayout new_layout);

        VulkanInstance instance_;
        VulkanDevice device_;
        VulkanBufferManager buffer_manager_;
        vk::UniqueCommandPool command_pool_;
        vk::UniqueSampler nearest_sampler_;
        vk::UniqueSampler linear_sampler_;

        std::mutex transfer_thread_mutex_;
        std::deque<std::shared_ptr<TextureUploadPayload>> pending_texture_uploads_;
        vk::UniqueCommandPool transfer_thread_command_pool_;
        std::atomic<bool> transfer_thread_running_{true};
        std::condition_variable transfer_thread_cv_;
        std::jthread transfer_thread_;
    };

    class VulkanTexture final : public Texture
    {
      public:
        inline VulkanTexture(RefCountPtr<VulkanRenderBackend> backend,
                             vk::UniqueImage image,
                             vk::UniqueDeviceMemory memory,
                             vk::UniqueImageView view,
                             const vk::Sampler sampler,
                             const std::int32_t width,
                             const std::int32_t height,
                             const TextureFormat format,
                             const TextureFilter filter) noexcept
            : Texture{width, height, format, filter}, render_backend_{std::move(backend)}, image_{std::move(image)},
              memory_{std::move(memory)}, view_{std::move(view)}, sampler_{sampler}
        {
        }

        [[nodiscard]] inline vk::Image image() const noexcept
        {
            return image_.get();
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
        RefCountPtr<VulkanRenderBackend> render_backend_{};
        vk::UniqueImage image_;
        vk::UniqueDeviceMemory memory_;
        vk::UniqueImageView view_;
        vk::Sampler sampler_;
    };
} // namespace retro
