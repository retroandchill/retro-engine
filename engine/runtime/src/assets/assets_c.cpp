/**
 * @file assets_c.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/macros.hpp"
#include "retro/runtime/assets/assets.h"

#include <boost/pool/pool_alloc.hpp>

import retro.runtime;
import retro.core.c_api;
import retro.core.strings.name;
import std;

DECLARE_OPAQUE_C_HANDLE(Retro_Asset, retro::Asset)
DECLARE_OPAQUE_C_HANDLE(Retro_Texture, retro::Texture);
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

uint8_t retro_asset_path_from_string(const char16_t *path, const int32_t length, Retro_AssetPath *out_path)
{
    try
    {
        *out_path = to_c(retro::AssetPath{std::u16string_view{path, static_cast<std::size_t>(length)}});
        return true;
    }
    catch (const std::invalid_argument &)
    {
        return false;
    }
}

uint8_t retro_asset_path_is_valid(const Retro_AssetPath *path)
{
    return from_c(path)->is_valid();
}

int32_t retro_asset_path_to_string(const Retro_AssetPath *path, char16_t *buffer, const int32_t length)
{
    const auto utf16_string = from_c(*path).to_string<char16_t>(boost::pool_allocator<char16_t>{});
    const std::size_t string_length = std::min(utf16_string.size(), static_cast<std::size_t>(length));
    std::memcpy(buffer, utf16_string.data(), string_length * sizeof(char16_t));
    return static_cast<std::int32_t>(string_length);
}

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

Retro_Vector2i retro_texture_get_size(const Retro_Texture *texture)
{
    auto &texture_ref = *from_c(texture);
    return Retro_Vector2i{
        .x = texture_ref.width(),
        .y = texture_ref.height(),
    };
}
