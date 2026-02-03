/**
 * @file filesystem_asset_source.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.assets.filesystem_asset_source;

import retro.platform.dll;
import retro.core.io.file_stream;

namespace retro
{
    AssetLoadResult<std::unique_ptr<Stream>> FileSystemAssetSource::open_stream(const AssetPath path,
                                                                                const AssetOpenOptions &options)
    {
        auto content_root = get_executable_path();
        // TODO: For now just assume the first part of the path is a folder relative to the content root
        content_root /= path.package_name().to_string();
        content_root /= path.asset_name().to_string();

        return FileStream::open(content_root, FileOpenMode::ReadOnly)
            .transform_error([](const auto &) { return AssetLoadError::AssetNotFound; })
            .transform([](auto &&file) { return std::unique_ptr<Stream>{std::move(file)}; });
    }
} // namespace retro
