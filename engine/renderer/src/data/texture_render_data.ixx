/**
 * @file texture_render_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.renderer:data.texture_render_data;

import vulkan_hpp;
import retro.runtime.rendering.texture_render_data;

namespace retro
{
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
} // namespace retro
