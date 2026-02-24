/**
 * @file texture_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer.vulkan.texture_manager;

import vulkan_hpp;
import retro.runtime.rendering.texture_manager;
import retro.renderer.vulkan.components.device;
import retro.core.di;

namespace retro
{
    export class VulkanTextureRenderData final : public TextureRenderData
    {
      public:
        inline VulkanTextureRenderData(vk::UniqueImage image,
                                       vk::UniqueDeviceMemory memory,
                                       vk::UniqueImageView view,
                                       const vk::Sampler sampler,
                                       const std::int32_t width,
                                       const std::int32_t height) noexcept
            : TextureRenderData{width, height}, image_{std::move(image)}, memory_{std::move(memory)},
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

    export class VulkanTextureManager final : public TextureManager
    {
      public:
        using Dependencies = TypeList<VulkanDevice &, vk::CommandPool>;

        VulkanTextureManager(VulkanDevice &device, vk::CommandPool command_pool);

        std::unique_ptr<TextureRenderData> upload_texture(const ImageData &image_data) override;

      private:
        [[nodiscard]] vk::UniqueCommandBuffer begin_one_shot_commands() const;
        void end_one_shot_commands(vk::UniqueCommandBuffer &&cmd) const;

        static void transition_image_layout(vk::CommandBuffer cmd,
                                            vk::Image image,
                                            vk::ImageLayout old_layout,
                                            vk::ImageLayout new_layout);

        VulkanDevice &device_;
        vk::CommandPool command_pool_;
        vk::UniqueSampler linear_sampler_;
    };
} // namespace retro
