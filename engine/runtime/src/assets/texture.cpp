/**
 * @file texture.cpp
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

module retro.runtime;

namespace retro
{
    namespace
    {
        constexpr std::array PNG_HEADER = {std::byte{0x89},
                                           std::byte{0x50},
                                           std::byte{0x4E},
                                           std::byte{0x47},
                                           std::byte{0x0D},
                                           std::byte{0x0A},
                                           std::byte{0x1A},
                                           std::byte{0x0A}};
    }

    bool TextureDecoder::can_decode(const AssetDecodeContext &context, BufferedStream &stream) const
    {
        // TODO: For now we'll just support PNG images, but we may want to open it up to others later
        auto peek_result = stream.peek(PNG_HEADER.size());
        if (!peek_result.has_value())
        {
            return false;
        }

        return std::ranges::equal(*peek_result, PNG_HEADER);
    }

    AssetLoadResult<RefCountPtr<Asset>> TextureDecoder::decode(const AssetDecodeContext &context,
                                                               BufferedStream &stream)
    {
        return stream.read_all()
            .transform_error([](StreamError) { return AssetLoadError::InvalidAssetFormat; })
            .and_then([](const std::vector<std::byte> &bytes) { return load_image_data(bytes); })
            .transform([this](const ImageData &image_data) { return renderer_->upload_texture(image_data); })
            .transform([&context](TextureRenderData &&render_data)
                       { return make_ref_counted<Texture>(context.path, std::move(render_data)); });
    }

    AssetLoadResult<ImageData> TextureDecoder::load_image_data(const std::span<const std::byte> bytes) noexcept
    {
        ImageData result{};
        auto *image_data = stbi_load_from_memory(reinterpret_cast<stbi_uc const *>(bytes.data()),
                                                 static_cast<int32>(bytes.size()),
                                                 &result.width,
                                                 &result.height,
                                                 &result.channels,
                                                 0);
        if (image_data == nullptr)
        {
            get_logger().error(stbi_failure_reason());
            return std::unexpected{AssetLoadError::InvalidAssetFormat};
        }

        result.image_data.reset(reinterpret_cast<std::byte *>(image_data));
        return std::move(result);
    }

    Name Texture::asset_type() const noexcept
    {
        static Name type{"Texture"};
        return type;
    }

    void ImageDataDeleter::operator()(std::byte *bytes) const
    {
        stbi_image_free(bytes);
    }
} // namespace retro
