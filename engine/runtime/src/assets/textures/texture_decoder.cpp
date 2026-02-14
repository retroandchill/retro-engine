/**
 * @file texture_decoder.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.assets.textures.texture_decoder;

import retro.core.io.stream;
import retro.runtime.assets.textures.texture;
import retro.runtime.rendering.texture_manager;
import retro.logging;

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
            .transform_error([](StreamError) { return AssetLoadError::invalid_asset_format; })
            .and_then([](const std::vector<std::byte> &bytes) { return load_image_data(bytes); })
            .transform([this](const ImageData &image_data) { return manager_.upload_texture(image_data); })
            .transform([&context](std::unique_ptr<TextureRenderData> &&render_data)
                       { return make_ref_counted<Texture>(context.path, std::move(render_data)); });
    }

    AssetLoadResult<ImageData> TextureDecoder::load_image_data(const std::span<const std::byte> bytes) noexcept
    {
        return ImageData::create_from_memory(bytes).transform_error(
            [](const std::string_view &error)
            {
                get_logger().error(error);
                return AssetLoadError::invalid_asset_format;
            });
    }
} // namespace retro
