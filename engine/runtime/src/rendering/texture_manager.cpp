/**
 * @file texture_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#ifdef _DEBUG
#define STBI_FAILURE_USERMSG
#endif
#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>

module retro.runtime.rendering.texture_manager;

namespace retro
{
    void ImageDataDeleter::operator()(std::byte *bytes) const
    {
        stbi_image_free(bytes);
    }

    std::expected<ImageData, std::string_view> ImageData::create_from_memory(std::span<const std::byte> bytes) noexcept
    {
        ImageData result{};
        std::int32_t channels;
        auto *image_data = stbi_load_from_memory(reinterpret_cast<stbi_uc const *>(bytes.data()),
                                                 static_cast<std::int32_t>(bytes.size()),
                                                 &result.width_,
                                                 &result.height_,
                                                 &channels,
                                                 4);

        if (image_data == nullptr)
        {
            return std::unexpected{stbi_failure_reason()};
        }

        result.channels_ = 4;
        result.image_data_.reset(reinterpret_cast<std::byte *>(image_data));

        return std::move(result);
    }
} // namespace retro
