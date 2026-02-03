/**
 * @file texture_render_data.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.texture_render_data;

import std;

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
        RETRO_API static std::expected<ImageData, std::string_view> create_from_memory(
            std::span<const std::byte> bytes) noexcept;

        [[nodiscard]] inline std::span<const std::byte> bytes() const noexcept
        {
            return std::span{image_data_.get(), static_cast<std::size_t>(width_ * height_ * channels_)};
        }

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return width_;
        }

        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return height_;
        }

        [[nodiscard]] inline std::int32_t channels() const noexcept
        {
            return channels_;
        }

      private:
        ImageDataPtr image_data_{};
        std::int32_t width_ = 0;
        std::int32_t height_ = 0;
        std::int32_t channels_ = 0;
    };

    export class TextureRenderData
    {
      protected:
        inline TextureRenderData(std::int32_t width, std::int32_t height) noexcept : width_(width), height_(height)
        {
        }

      public:
        virtual ~TextureRenderData() = default;

        [[nodiscard]] inline std::int32_t width() const noexcept
        {
            return width_;
        }
        [[nodiscard]] inline std::int32_t height() const noexcept
        {
            return height_;
        }

      private:
        std::int32_t width_{};
        std::int32_t height_{};
    };
} // namespace retro
