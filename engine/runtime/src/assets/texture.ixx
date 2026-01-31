/**
 * @file texture.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:texture;

import :assets;

namespace retro
{
    struct ImageDataDeleter
    {
        RETRO_API void operator()(std::byte *bytes) const;
    };

    using ImageDataPtr = std::unique_ptr<std::byte[], ImageDataDeleter>;

    struct ImageData
    {
        ImageDataPtr image_data;
        int32 width;
        int32 height;
        int32 channels;
    };

    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        [[nodiscard]] bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const override;

        AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context, BufferedStream &stream) override;

      private:
        static AssetLoadResult<ImageData> load_image_data(std::span<const std::byte> bytes) noexcept;
    };

    export class RETRO_API Texture final : public Asset
    {
      public:
        explicit Texture(const AssetPath &path, ImageData image_data) : Asset(path), image_data_{std::move(image_data)}
        {
        }

        [[nodiscard]] Name asset_type() const noexcept override;

      private:
        friend class TextureDecoder;

        ImageData image_data_;
    };
} // namespace retro
