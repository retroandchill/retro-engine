/**
 * @file filesystem_asset_source.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.assets.filesystem_asset_source;

import std;
import retro.core.io.stream;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset_manager;
import retro.runtime.assets.asset_source;
import retro.runtime.assets.asset_load_result;

namespace retro
{
    export class RETRO_API FileSystemAssetSource final : public AssetSource
    {
      public:
        AssetLoadResult<std::unique_ptr<Stream>> open_stream(AssetPath path, const AssetOpenOptions &options) override;
    };
} // namespace retro
