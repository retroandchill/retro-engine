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
import :scene.rendering;

namespace retro
{
    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        using Dependencies = TypeList<Renderer2D>;

        explicit inline TextureDecoder(Renderer2D &renderer) : renderer_{&renderer}
        {
        }

        [[nodiscard]] bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const override;

        AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context, BufferedStream &stream) override;

      private:
        static AssetLoadResult<ImageData> load_image_data(std::span<const std::byte> bytes) noexcept;

        Renderer2D *renderer_;
    };

    export class RETRO_API Texture final : public Asset
    {
      public:
        explicit Texture(const AssetPath &path, TextureRenderData render_data)
            : Asset(path), render_data_{std::move(render_data_)}
        {
        }

        [[nodiscard]] Name asset_type() const noexcept override;

      private:
        friend class TextureDecoder;

        TextureRenderData render_data_;
    };
} // namespace retro
