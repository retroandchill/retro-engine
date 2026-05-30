/**
 * @file image_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.image_data;

import std;
import stb.image;
import retro.runtime.rendering.texture;

namespace retro
{
    export class ImageData
    {
        inline ImageData(stb::image::Image image) : image_{std::move(image)}
        {
        }

      public:
        RETRO_API static ImageData create_from_memory(std::span<const std::byte> bytes) noexcept;

        [[nodiscard]] inline std::span<const std::byte> bytes() const noexcept
        {
            return image_.bytes();
        }

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return image_.width();
        }

        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return image_.height();
        }

        [[nodiscard]] inline TextureFormat format() const noexcept
        {
            return format_;
        }

      private:
        [[nodiscard]] static constexpr std::size_t texture_format_to_channels(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat::rgba8:
                case TextureFormat::unorm:
                    return 4;
                case TextureFormat::rgba16f:
                    return 8;
                default:
                    return 0;
            }
        }

        stb::image::Image image_;
        TextureFormat format_ = TextureFormat::rgba8;
    };
} // namespace retro
