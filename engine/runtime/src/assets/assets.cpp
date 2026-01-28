/**
 * @file assets.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

import retro.platform;

namespace retro
{
    AssetLoadResult<std::unique_ptr<Stream>> FileSystemAssetSource::open_stream(AssetPath path,
                                                                                const AssetOpenOptions &options)
    {
        auto content_root = get_executable_path();
        // TODO: For now just assume the first part of the path is a folder relative to the content root
        content_root /= path.package_name().to_string();
        content_root /= path.asset_name().to_string();

        return FileStream::open(content_root, FileOpenMode::ReadOnly)
            .transform_error([](const auto &) { return AssetLoadError{std::in_place_type<AssetNotFound>}; })
            .transform([](auto &&file) { return std::unique_ptr<Stream>{std::move(file)}; });
    }

    AssetLoadResult<RefCountPtr<Asset>> AssetLoader::load_asset_from_path(AssetPath path)
    {
        return open_stream(path).and_then([this, path](const std::unique_ptr<Stream> &stream)
                                          { return load_asset_from_stream(path, *stream); });
    }

    std::expected<RefCountPtr<Asset>, AssetLoadError> AssetManager::load_asset_internal(const AssetPath &path)
    {
        if (const auto existing_asset = asset_cache_.find(path); existing_asset != asset_cache_.end())
        {
            return RefCountPtr{existing_asset->second};
        }

        return asset_loader_->load_asset_from_path(path);
    }
} // namespace retro
