/**
 * @file asset_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.runtime;

namespace retro
{
    std::expected<RefCountPtr<Asset>, AssetLoadError> AssetManager::load_asset_internal(const AssetPath &path)
    {
        if (const auto existing_asset = asset_cache_.find(path); existing_asset != asset_cache_.end())
        {
            return RefCountPtr{&*existing_asset};
        }

        return asset_loader_->load_asset_from_path(path);
    }
} // namespace retro
