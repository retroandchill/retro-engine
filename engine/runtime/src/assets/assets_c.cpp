/**
 * @file assets_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/runtime/assets/assets.h"

import retro.runtime;
import std;

namespace
{
    Retro_AssetHandle to_c(retro::Asset *asset)
    {
        return reinterpret_cast<std::uintptr_t>(asset);
    }

    retro::Asset *from_c(const Retro_AssetHandle node) noexcept
    {
        return reinterpret_cast<retro::Asset *>(static_cast<std::uintptr_t>(node));
    }

    const retro::AssetPath &from_c(const Retro_AssetPath *path)
    {
        static_assert(sizeof(Retro_AssetPath) == sizeof(retro::AssetPath));
        return *reinterpret_cast<const retro::AssetPath *>(path);
    }

    Retro_AssetLoadError to_c(retro::AssetLoadError error)
    {
        switch (error)
        {
            case retro::AssetLoadError::BadAssetPath:
                return Retro_BadAssetPath;
            case retro::AssetLoadError::InvalidAssetFormat:
                return Retro_InvalidAssetFormat;
            case retro::AssetLoadError::AmbiguousAssetPath:
                return Retro_AmbiguousAssetPath;
            case retro::AssetLoadError::AssetNotFound:
                return Retro_AssetNotFound;
            case retro::AssetLoadError::AssetTypeMismatch:
                return Retro_AssetTypeMismatch;
            default:
                return static_cast<Retro_AssetLoadError>(error);
        }
    }

    Retro_Name to_c(const retro::Name name)
    {
        return std::bit_cast<Retro_Name>(name);
    }
} // namespace

Retro_AssetHandle retro_load_asset(const Retro_AssetPath *path,
                                   Retro_Name *out_asset_type,
                                   Retro_AssetLoadError *out_error)
{
    auto result = retro::Engine::instance().load_asset(from_c(path));
    if (!result.has_value())
    {
        *out_error = to_c(result.error());
        return to_c(nullptr);
    }

    *out_asset_type = to_c((*result)->asset_type());
    (*result)->retain();
    return to_c(result->get());
}

void retro_release_asset(const Retro_AssetHandle asset)
{
    from_c(asset)->release();
}
