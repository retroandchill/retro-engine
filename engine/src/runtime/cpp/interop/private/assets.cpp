/**
 * @file assets.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

#include <boost/pool/pool_alloc.hpp>

import retro.runtime.engine;
import retro.core.strings.name;
import retro.runtime.assets.asset_path;
import retro.core.math.vector;
import std;
import retro.runtime.rendering.texture;

namespace retro
{
    struct CVector2i
    {
        std::int32_t x = 0;
        std::int32_t y = 0;
    };
} // namespace retro

extern "C"
{
    RETRO_API bool retro_asset_path_from_string(const char16_t *path, const int32_t length, retro::AssetPath *out_path)
    {
        try
        {
            *out_path = retro::AssetPath{std::u16string_view{path, static_cast<std::size_t>(length)}};
            return true;
        }
        catch (const std::invalid_argument &)
        {
            return false;
        }
    }

    RETRO_API bool retro_asset_path_is_valid(const retro::AssetPath *path)
    {
        return path->is_valid();
    }

    RETRO_API std::int32_t retro_asset_path_to_string(const retro::AssetPath *path,
                                                      char16_t *buffer,
                                                      const int32_t length)
    {
        const auto utf16_string = path->to_string<char16_t>(boost::pool_allocator<char16_t>{});
        const std::size_t string_length = std::min(utf16_string.size(), static_cast<std::size_t>(length));
        std::memcpy(buffer, utf16_string.data(), string_length * sizeof(char16_t));
        return static_cast<std::int32_t>(string_length);
    }

    RETRO_API void retro_texture_render_data_destroy(retro::TextureRenderData *texture)
    {
        delete texture;
    }

    RETRO_API void retro_texture_destroy(retro::Texture *texture)
    {
        delete texture;
    }

    RETRO_API retro::CVector2i retro_texture_get_size(const retro::Texture *texture)
    {
        return retro::CVector2i{
            .x = texture->width(),
            .y = texture->height(),
        };
    }
}
