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
    Optional<RefCountPtr<Asset>> AssetManager::load_asset_internal(const AssetPath &path,
                                                                   std::type_index type_index,
                                                                   std::span<const std::byte> buffer)
    {
        {
            std::shared_lock lock{asset_cache_mutex_};
            if (const auto existing_asset = asset_cache_.find(path); existing_asset != asset_cache_.end())
            {
                return RefCountPtr<Asset>::ref(existing_asset->second);
            }
        }

        auto decoder = decoders_.find(type_index);
        if (decoder == decoders_.end())
        {
            get_logger().error("Asset type {} is not supported", type_index.name());
            return std::nullopt;
        }

        const AssetDecodeContext context{.path = path, .bytes = buffer};
        auto decoded = decoder->second->decode(context);
        if (!decoded.has_value())
        {
            return std::nullopt;
        }

        std::unique_lock lock{asset_cache_mutex_};
        asset_cache_[path] = decoded->get();
        return std::move(decoded);
    }

    AssetManager::AssetManager(const std::vector<AssetDecoder *> &decoders)
        : decoders_{std::from_range,
                    decoders | std::views::transform([](AssetDecoder *decode)
                                                     { return std::make_pair(decode->asset_type(), decode); })}
    {
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

    Asset *AssetManager::load_asset_from_cache(const AssetPath &path)
    {
        std::shared_lock lock{asset_cache_mutex_};
        if (const auto existing_asset = asset_cache_.find(path); existing_asset != asset_cache_.end())
        {
            return existing_asset->second;
        }

        return nullptr;
    }
} // namespace retro
