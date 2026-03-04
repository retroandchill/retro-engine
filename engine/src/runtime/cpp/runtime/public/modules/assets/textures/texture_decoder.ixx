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
import retro.runtime.rendering.texture_manager;
import retro.runtime.assets.asset;
import retro.core.memory.ref_counted_ptr;
import retro.core.io.buffered_stream;
import retro.runtime.rendering.renderer2d;
import retro.runtime.assets.textures.texture;
import retro.core.containers.optional;
import std;

namespace retro
{
    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        using Dependencies = TypeList<TextureManager &>;

        explicit inline TextureDecoder(TextureManager &renderer) : AssetDecoder{typeid(Texture)}, manager_{renderer}
        {
        }

        Optional<RefCountPtr<Asset>> decode(const AssetDecodeContext &context) override;

      private:
        static Optional<ImageData> load_image_data(std::span<const std::byte> bytes) noexcept;

        TextureManager &manager_;
    };
} // namespace retro
