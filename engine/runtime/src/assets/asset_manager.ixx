/**
 * @file asset_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime:assets.asset_manager;

import retro.core;
import :assets.asset;
import :assets.asset_loader;

namespace retro
{
    export class RETRO_API AssetManager
    {
      public:
        explicit AssetManager(std::unique_ptr<AssetLoader> asset_loader) : asset_loader_{std::move(asset_loader)}
        {
        }

        template <std::derived_from<Asset> T>
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
                        auto *asset_ptr = asset.get();
                        auto cast_ptr = dynamic_pointer_cast<T>(std::move(asset));
                        if (cast_ptr == nullptr)
                        {
                            return std::unexpected{
                                AssetLoadError{std::in_place_type<AssetTypeMismatch>, typeid(T), typeid(*asset_ptr)}};
                        }

                        return std::move(cast_ptr);
                    });
                return retro::dynamic_pointer_cast<T>();
            }
        }

      private:
        std::expected<RefCountPtr<Asset>, AssetLoadError> load_asset_internal(const AssetPath &path);

        std::unique_ptr<AssetLoader> asset_loader_{};
        Asset::Map asset_cache_{};
    };
} // namespace retro
