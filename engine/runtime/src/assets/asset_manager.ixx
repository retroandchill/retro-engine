/**
 * @file assets.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.assets.asset_manager;

import retro.core.di;
import retro.core.strings.name;
import retro.core.algorithm.hashing;
import retro.core.type_traits.basic;
import retro.core.io.stream;
import retro.core.memory.ref_counted_ptr;
import std;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset;
import retro.runtime.assets.asset_load_result;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_decoder;

namespace retro
{

    export class RETRO_API AssetManager
    {
      public:
        using Dependencies = TypeList<AssetSource, AssetDecoder>;

        explicit inline AssetManager(AssetSource &asset_source, std::vector<std::shared_ptr<AssetDecoder>> decoders)
            : asset_source_{std::addressof(asset_source)}, decoders_{std::move(decoders)}
        {
        }

        template <std::derived_from<Asset> T = Asset>
        std::expected<RefCountPtr<T>, AssetLoadError> load_asset(const AssetPath &path)
        {
            if constexpr (std::is_same_v<T, Asset>)
            {
                return load_asset_internal(path);
            }
            else
            {
                return load_asset_internal(path).and_then(
                    [](RefCountPtr<Asset> &&asset) -> std::expected<RefCountPtr<T>, AssetLoadError>
                    {
                        auto cast_ptr = dynamic_pointer_cast<T>(std::move(asset));
                        if (cast_ptr == nullptr)
                        {
                            return std::unexpected{AssetLoadError::AssetTypeMismatch};
                        }

                        return std::move(cast_ptr);
                    });
            }
        }

        bool remove_asset_from_cache(const AssetPath &path);

        void on_engine_shutdown();

      private:
        AssetLoadResult<RefCountPtr<Asset>> load_asset_internal(const AssetPath &path);
        AssetLoadResult<RefCountPtr<Asset>> load_asset_from_stream(const AssetPath &path, Stream &stream);

        AssetSource *asset_source_{};
        std::vector<std::shared_ptr<AssetDecoder>> decoders_;
        std::shared_mutex asset_cache_mutex_{};
        std::unordered_map<AssetPath, Asset *> asset_cache_{};
    };
} // namespace retro
