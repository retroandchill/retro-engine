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
        RefCountPtr<T> load_asset(const AssetPath &path)
        {
            if constexpr (std::is_same_v<T, Asset>)
            {
                return load_asset_internal(path);
            }
            else
            {
                return retro::dynamic_pointer_cast<T>(load_asset_internal(path));
            }
        }

      private:
        RefCountPtr<Asset> load_asset_internal(const AssetPath &path);

        std::unique_ptr<AssetLoader> asset_loader_{};
        Asset::Map asset_cache_{};
    };
} // namespace retro
