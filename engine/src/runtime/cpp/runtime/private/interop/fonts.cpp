/**
 * @file fonts.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.rendering.text.font;
import retro.core.interop.interop_error;
import retro.runtime.rendering.render_backend;

using namespace retro;

extern "C"
{
    RETRO_API FontService *retro_font_service_create(RenderBackend *render_backend, InteropError *error)
    {
        return try_execute([render_backend] { return new FontService(*render_backend); }, *error);
    }

    RETRO_API void retro_font_service_destroy(const FontService *service)
    {
        delete service;
    }

    RETRO_API Font *retro_font_service_load_font(const FontService *service,
                                                 const std::byte *bytes,
                                                 const std::int32_t size,
                                                 InteropError *error)
    {
        return try_execute(
            [=]
            {
                auto input_span = std::span{bytes, static_cast<std::size_t>(size)};
                std::vector buffer(input_span.begin(), input_span.end());
                return service->load_font(std::move(buffer)).release();
            },
            *error);
    }

    RETRO_API void retro_font_destroy(const Font *font)
    {
        font->sub_ref();
    }

    RETRO_API const char *retro_font_get_family_name(const Font *font, std::int32_t *length)
    {
        const auto name = font->face().family_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }

    RETRO_API const char *retro_font_get_style_name(const Font *font, std::int32_t *length)
    {
        const auto name = font->face().style_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }
}
