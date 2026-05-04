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

namespace retro
{
    export class VulkanTexture;

    export class VulkanRenderBackend final : public RenderBackend
    {
      public:
        explicit VulkanRenderBackend(PlatformBackend &platform_backend);

        std::shared_ptr<Renderer2D> create_renderer(std::unique_ptr<Window> window) override;

        RefCountPtr<Texture> upload_texture(std::span<const std::byte> bytes,
                                            std::int32_t width,
                                            std::int32_t height,
                                            TextureFormat format) override;

        std::pair<bool, std::size_t> export_texture(const Texture &texture, std::span<std::byte> buffer) override;

      private:
        vk::CommandPool get_thread_command_pool() const;

        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands(vk::CommandPool command_pool) const;
        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

        static void transition_image_layout(vk::CommandBuffer cmd,
                                            vk::Image image,
                                            vk::ImageLayout old_layout,
                                            vk::ImageLayout new_layout);

      private:
        VulkanInstance instance_;
        VulkanDevice device_;
        VulkanBufferManager buffer_manager_;
        vk::UniqueCommandPool command_pool_;
        vk::UniqueSampler linear_sampler_;

        mutable std::shared_mutex thread_pools_mutex_;
        mutable std::unordered_map<std::thread::id, vk::UniqueCommandPool> thread_pools_;
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
                             const TextureFormat format) noexcept
            : Texture{width, height, format}, render_backend_{std::move(backend)}, image_{std::move(image)},
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
