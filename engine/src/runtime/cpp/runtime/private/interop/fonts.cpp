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
import retro.core.memory.ref_counted_ptr;

using namespace retro;

namespace
{
    using OnFontLoaded = void (*)(void *, Font *);
    using OnFontLoadFailed = void (*)(void *, InteropError);
    using OnFontLoadCancelled = void (*)(void *);
} // namespace

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

    RETRO_API void retro_font_service_load_font(const FontService *service,
                                                const std::byte *bytes,
                                                const std::int32_t size,
                                                OnFontLoaded on_loaded,
                                                OnFontLoadFailed on_failed,
                                                OnFontLoadCancelled on_cancelled,
                                                void *user_data,
                                                const std::stop_source *stop_source)
    {
        const auto stop_token = stop_source != nullptr ? stop_source->get_token() : std::stop_token{};
        try_execute_async(
            [=]
            {
                auto input_span = std::span{bytes, static_cast<std::size_t>(size)};
                std::vector buffer(input_span.begin(), input_span.end());
                return service->load_font(std::move(buffer), stop_token).configure_await(false);
            },
            [on_loaded, user_data](RefCountPtr<Font> font) { on_loaded(user_data, font.release()); },
            [on_failed, user_data](const InteropError &error) { on_failed(user_data, error); },
            [on_cancelled, user_data] { on_cancelled(user_data); },
            stop_token);
    }

    RETRO_API void retro_font_destroy(const Font *font)
    {
        font->sub_ref();
    }

    RETRO_API const char *retro_font_get_family_name(const Font *font, std::int32_t *length)
    {
        const auto name = font->family_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }

    RETRO_API const char *retro_font_get_style_name(const Font *font, std::int32_t *length)
    {
        const auto name = font->style_name();
        *length = static_cast<std::int32_t>(name.size());
        return name.data();
    }
}
