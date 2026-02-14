/**
 * @file asset_source.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.assets.asset_source;

import std;
import retro.core.io.stream;
import retro.runtime.assets.asset_path;
import retro.runtime.assets.asset_load_result;

namespace retro
{
    export struct AssetOpenOptions
    {
        // TODO: Either remove or add options
    };

    export class AssetSource
    {
      public:
        virtual ~AssetSource() = default;

        inline AssetLoadResult<std::unique_ptr<Stream>> open_stream(const AssetPath &path)
        {
            return open_stream(path, {});
        }

        virtual AssetLoadResult<std::unique_ptr<Stream>> open_stream(AssetPath path,
                                                                     const AssetOpenOptions &options) = 0;
    };
} // namespace retro
