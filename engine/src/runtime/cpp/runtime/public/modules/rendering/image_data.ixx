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
import retro.runtime.rendering.texture;

namespace retro
{

    struct ImageDataDeleter
    {
        RETRO_API void operator()(std::byte *bytes) const;
    };

    using ImageDataPtr = std::unique_ptr<std::byte[], ImageDataDeleter>;

    export class ImageData
    {
        ImageData() = default;

      public:
        RETRO_API static ImageData create_from_memory(std::span<const std::byte> bytes) noexcept;

        [[nodiscard]] inline std::span<const std::byte> bytes() const noexcept
        {
            const auto num_channels = texture_format_to_channels(format_);
            if (num_channels == 0)
                return {};

            return std::span{image_data_.get(), width_ * height_ * num_channels};
        }

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return width_;
        }

        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return height_;
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
                    return 4;
                case TextureFormat::rgba16f:
                    return 8;
                default:
                    return 0;
            }
        }

        ImageDataPtr image_data_{};
        std::int32_t width_ = 0;
        std::int32_t height_ = 0;
        TextureFormat format_ = TextureFormat::rgba8;
    };
} // namespace retro
