/**
 * @file asset_manager.ixx
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
import retro.core.containers.optional;
import retro.core.memory.ref_counted_ptr;
import std;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset;
import retro.runtime.assets.asset_load_result;
import retro.runtime.assets.asset_decoder;
import retro.logging;

namespace retro
{

    export class RETRO_API AssetManager
    {
      public:
        using Dependencies = TypeList<const std::vector<AssetDecoder *> &>;

        explicit AssetManager(const std::vector<AssetDecoder *> &decoders);

        template <std::derived_from<Asset> T = Asset>
        Optional<T &> load_from_cache(const AssetPath &path)
        {
            if constexpr (std::is_same_v<T, Asset>)
            {
                return load_asset_from_cache(path);
            }
            else
            {
                auto asset = load_asset_from_cache(path);
                if (asset == nullptr)
                {
                    return std::nullopt;
                }

                auto *cast_pointer = dynamic_cast<T *>(asset);
                if (cast_pointer == nullptr)
                {
                    get_logger().error("Can't cast asset of type {} to type {}",
                                       typeid(*asset).name(),
                                       typeid(T).name());
                    return std::nullopt;
                }

                return *cast_pointer;
            }
        }

        template <std::derived_from<Asset> T = Asset>
        Optional<RefCountPtr<T>> load_asset(const AssetPath &path, const std::span<const std::byte> buffer)
        {
            if constexpr (std::is_same_v<T, Asset>)
            {
                return load_asset_internal(path, typeid(T), buffer);
            }
            else
            {
                return load_asset_internal(path, typeid(T), buffer)
                    .and_then(
                        [](RefCountPtr<Asset> &&asset) -> Optional<RefCountPtr<T>>
                        {
                            auto cast_ptr = retro::dynamic_pointer_cast<T>(std::move(asset));
                            if (cast_ptr == nullptr)
                            {
                                get_logger().error("Can't cast asset of type {} to type {}",
                                                   typeid(*asset).name(),
                                                   typeid(T).name());
                                return std::nullopt;
                            }

                            return std::move(cast_ptr);
                        });
            }
        }

        bool remove_asset_from_cache(const AssetPath &path);

        void on_engine_shutdown();

      private:
        Asset *load_asset_from_cache(const AssetPath &path);
        Optional<RefCountPtr<Asset>> load_asset_internal(const AssetPath &path,
                                                         std::type_index type_index,
                                                         std::span<const std::byte> buffer);

        std::unordered_map<std::type_index, AssetDecoder *> decoders_{};
        std::shared_mutex asset_cache_mutex_{};
        std::unordered_map<AssetPath, Asset *> asset_cache_{};
    };
} // namespace retro
