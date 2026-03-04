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
    Optional<RefCountPtr<Asset>> TextureDecoder::decode(const AssetDecodeContext &context)
    {
        return load_image_data(context.bytes)
            .transform([this](const ImageData &image_data) { return manager_.upload_texture(image_data); })
            .transform([&context](std::unique_ptr<TextureRenderData> &&render_data)
                       { return make_ref_counted<Texture>(context.path, std::move(render_data)); });
    }

    Optional<ImageData> TextureDecoder::load_image_data(const std::span<const std::byte> bytes) noexcept
    {
        auto result = ImageData::create_from_memory(bytes);
        if (!result.has_value())
        {
            get_logger().error("Failed to load image data: {}", result.error());
            return std::nullopt;
        }

        return *std::move(result);
    }
} // namespace retro
