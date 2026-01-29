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
    export class RETRO_API TextureDecoder final : public AssetDecoder
    {
      public:
        [[nodiscard]] bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const override;

        AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context, BufferedStream &stream) override;
    };
} // namespace retro
