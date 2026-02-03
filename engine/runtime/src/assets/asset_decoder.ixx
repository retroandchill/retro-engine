/**
 * @file asset_decoder.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.assets.asset_decoder;

import retro.runtime.assets.asset_path;

import retro.runtime.assets.asset;
import retro.runtime.assets.asset_load_result;
import retro.core.memory.ref_counted_ptr;
import retro.core.io.buffered_stream;

namespace retro
{
    export struct AssetDecodeContext
    {
        AssetPath path{};
    };

    export class AssetDecoder
    {
      public:
        virtual ~AssetDecoder() = default;

        [[nodiscard]] virtual bool can_decode(const AssetDecodeContext &context, BufferedStream &stream) const = 0;

        virtual AssetLoadResult<RefCountPtr<Asset>> decode(const AssetDecodeContext &context,
                                                           BufferedStream &stream) = 0;
    };
} // namespace retro
