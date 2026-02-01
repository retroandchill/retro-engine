/**
 * @file assets_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/macros.hpp"
#include "retro/runtime/assets/assets.h"

import retro.core;
import retro.runtime;
import std;

DECLARE_OPAQUE_C_HANDLE(Retro_Asset, retro::Asset);
DECLARE_DEFINED_C_HANDLE(Retro_AssetPath, retro::AssetPath);
DECLARE_DEFINED_C_HANDLE(Retro_Name, retro::Name);

using retro::from_c;
using retro::to_c;

namespace
{
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
} // namespace

Retro_Asset *retro_load_asset(const Retro_AssetPath *path, Retro_Name *out_asset_type, Retro_AssetLoadError *out_error)
{
    auto result = retro::Engine::instance().load_asset(*from_c(path));
    if (!result.has_value())
    {
        *out_error = to_c(result.error());
        return nullptr;
    }

    *out_asset_type = to_c((*result)->asset_type());
    (*result)->retain();
    return to_c(result->get());
}

void retro_release_asset(Retro_Asset *asset)
{
    from_c(asset)->release();
}
