/**
 * @file assets.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"
#include "retro/core/strings/name.h"

#include <stdint.h> // NOLINT We want to use a C header here

#if __cplusplus
extern "C"
{
#endif

    typedef struct Retro_Asset Retro_Asset;

    typedef struct Retro_AssetPath
    {
        Retro_Name package_name;
        Retro_Name asset_name;
    } Retro_AssetPath;

    enum Retro_AssetLoadErrorEnum
    {
        Retro_BadAssetPath,
        Retro_InvalidAssetFormat,
        Retro_AmbiguousAssetPath,
        Retro_AssetNotFound,
        Retro_AssetTypeMismatch
    };

    typedef uint8_t Retro_AssetLoadError;

    RETRO_API Retro_Asset *retro_load_asset(const Retro_AssetPath *path,
                                            Retro_Name *out_asset_type,
                                            Retro_AssetLoadError *out_error);

    RETRO_API void retro_release_asset(Retro_Asset *asset);

#ifdef __cplusplus
}
#endif
