/**
 * @file asset_decoder.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.assets.asset_decoder;

import retro.runtime.assets.asset_path;

import std;
import retro.runtime.assets.asset;
import retro.runtime.assets.asset_load_result;
import retro.core.memory.ref_counted_ptr;
import retro.core.io.buffered_stream;
import retro.core.strings.name;
import retro.core.containers.optional;

namespace retro
{
    export struct AssetDecodeContext
    {
        AssetPath path{};
        std::span<const std::byte> bytes;
    };

    export class AssetDecoder
    {
      protected:
        explicit inline AssetDecoder(const std::type_index asset_type) noexcept : asset_type_{asset_type}
        {
        }

      public:
        virtual ~AssetDecoder() = default;

        inline std::type_index asset_type() const noexcept
        {
            return asset_type_;
        }

        virtual Optional<RefCountPtr<Asset>> decode(const AssetDecodeContext &context) = 0;

      private:
        std::type_index asset_type_;
    };
} // namespace retro
