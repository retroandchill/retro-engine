/**
 * @file asset_load_result.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.assets.asset_load_result;

import std;

namespace retro
{
    export enum class AssetLoadError : std::uint8_t
    {
        bad_asset_path,
        invalid_asset_format,
        ambiguous_asset_path,
        asset_not_found,
        asset_type_mismatch
    };

    export template <typename T>
    using AssetLoadResult = std::expected<T, AssetLoadError>;
} // namespace retro
