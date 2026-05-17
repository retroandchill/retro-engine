/**
 * @file fonts.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import std;
import retro.runtime.rendering.text.font_service;
import retro.core.interop.interop_error;

using namespace retro;

extern "C"
{
    RETRO_API FontService *retro_font_service_create(InteropError *error)
    {
        return try_execute([] { return new FontService(); }, *error);
    }

    RETRO_API void retro_font_service_destroy(const FontService *service)
    {
        delete service;
    }

    RETRO_API FontFace *retro_font_service_load_font(const FontService *service,
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

    RETRO_API void retro_font_face_destroy(const FontFace *face)
    {
        face->sub_ref();
    }

    RETRO_API const char *retro_font_face_get_family_name(const FontFace *face, std::int32_t *length)
    {
        const auto name = face->family_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }

    RETRO_API const char *retro_font_face_get_style_name(const FontFace *face, std::int32_t *length)
    {
        const auto name = face->style_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }
}
