/**
 * @file asset_loader.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:assets.asset_loader;

import retro.core;
import :assets.asset;

namespace retro
{
    export class AssetLoader
    {
      public:
        virtual ~AssetLoader() = default;

        virtual RefCountPtr<Asset> load_asset_from_path(AssetPath path) = 0;
    };
} // namespace retro
