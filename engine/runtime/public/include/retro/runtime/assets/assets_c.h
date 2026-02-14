/**
 * @file assets_c.h
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#pragma once

#include "retro/core/exports.h"
#include "retro/core/math/vector_c.h"
#include "retro/core/strings/name_c.h"

#include <stdint.h> // NOLINT We want to use a C header here

#if __cplusplus
extern "C"
{
#endif

    typedef struct Retro_Asset Retro_Asset;
    typedef struct Retro_Texture Retro_Texture;

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

    RETRO_API uint8_t retro_asset_path_from_string(const char16_t *path, int32_t length, Retro_AssetPath *out_path);

    RETRO_API uint8_t retro_asset_path_is_valid(const Retro_AssetPath *path);

    RETRO_API int32_t retro_asset_path_to_string(const Retro_AssetPath *path, char16_t *buffer, int32_t length);

    RETRO_API Retro_Asset *retro_load_asset(const Retro_AssetPath *path,
                                            Retro_Name *out_asset_type,
                                            Retro_AssetLoadError *out_error);

    RETRO_API void retro_release_asset(Retro_Asset *asset);

    RETRO_API Retro_Vector2i retro_texture_get_size(const Retro_Texture *texture);

#ifdef __cplusplus
}
#endif
