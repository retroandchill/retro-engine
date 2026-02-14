/**
 * @file texture_decoder.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.assets.textures.texture_decoder;

import retro.core.di;
import retro.runtime.assets.asset_decoder;
import retro.runtime.assets.asset_load_result;
import retro.runtime.rendering.texture_render_data;
import retro.runtime.assets.asset;
import retro.core.memory.ref_counted_ptr;
import retro.core.io.buffered_stream;
import retro.runtime.rendering.renderer2d;
import std;

namespace retro
{
    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        using Dependencies = TypeList<Renderer2D &>;

        explicit inline TextureDecoder(Renderer2D &renderer) : renderer_{&renderer}
        {
        }

        [[nodiscard]] bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const override;

        AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context, BufferedStream &stream) override;

      private:
        static AssetLoadResult<ImageData> load_image_data(std::span<const std::byte> bytes) noexcept;

        Renderer2D *renderer_;
    };
} // namespace retro
