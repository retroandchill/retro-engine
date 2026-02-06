/**
 * @file asset_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/macros.hpp"

module retro.runtime.assets.asset_manager;

import retro.core.io.buffered_stream;

namespace retro
{
    AssetLoadResult<RefCountPtr<Asset>> AssetManager::load_asset_internal(const AssetPath &path)
    {
        {
            std::shared_lock lock{asset_cache_mutex_};
            if (const auto existing_asset = asset_cache_.find(path); existing_asset != asset_cache_.end())
            {
                return RefCountPtr{existing_asset->second};
            }
        }

        return asset_source_->open_stream(path).and_then([this, path](const std::unique_ptr<Stream> &stream)
                                                         { return load_asset_from_stream(path, *stream); });
    }

    AssetLoadResult<RefCountPtr<Asset>> AssetManager::load_asset_from_stream(const AssetPath &path, Stream &stream)
    {
        BufferedStream buffered_stream{stream};
        for (const AssetDecodeContext context{.path = path};
             const auto &decoder : decoders_ | std::views::filter([&context, &buffered_stream](const AssetDecoder *d)
                                                                  { return d->can_decode(context, buffered_stream); }))
        {
            EXPECT_ASSIGN(auto decoded, decoder->decode(context, buffered_stream));
            std::unique_lock lock{asset_cache_mutex_};
            asset_cache_[path] = decoded.get();
            return std::move(decoded);
        }

        return std::unexpected{AssetLoadError::InvalidAssetFormat};
    }

    bool AssetManager::remove_asset_from_cache(const AssetPath &path)
    {
        std::unique_lock lock{asset_cache_mutex_};
        return asset_cache_.erase(path) == 1;
    }

    void AssetManager::on_engine_shutdown()
    {
        std::unique_lock lock{asset_cache_mutex_};
        for (auto *asset : asset_cache_ | std::views::values)
        {
            asset->on_engine_shutdown();
        }

        asset_cache_.clear();
    }
} // namespace retro
