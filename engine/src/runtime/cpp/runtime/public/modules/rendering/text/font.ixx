/**
 * @file font.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.text.font;

import std;
import retro.core.util.noncopyable;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.texture;
import retro.runtime.rendering.render_backend;
import retro.core.containers.optional;
import retro.runtime.rendering.layout.uvs;
import retro.core.async.task;
import :freetype;
import :async_atlas_generator;
import :async_dynamic_atlas;

namespace retro
{
    export class Font;
    export class FontService;

    struct FontHandleDeleter
    {
        void operator()(msdfgen::FontHandle *handle) const noexcept
        {
            msdfgen::destroyFont(handle);
        }
    };

    using FontHandlePtr = std::unique_ptr<msdfgen::FontHandle, FontHandleDeleter>;

    FontHandlePtr create_font_handle(FT_Face face)
    {
        return FontHandlePtr{msdfgen::adoptFreetypeFont(face)};
    }

    export struct FontMsdfAtlasConfig
    {
        float pixel_size{64};
        float distance_range{2.0f};
    };

    export struct FontMetrics
    {
        float ascender{};
        float descender{};
        float line_height{};
        float underline_position{};
        float underline_thickness{};
    };

    export struct GlyphMetrics
    {
        char32_t codepoint{};
        std::uint32_t glyph_index{};

        float advance_x{};
        float bearing_x{};
        float bearing_y{};

        float width{};
        float height{};

        UVs uvs{};
    };

    using FontAtlasData = AsyncDynamicAtlas<
        AsyncAtlasGenerator<float, 4, msdf_atlas::mtsdfGenerator, msdf_atlas::BitmapAtlasStorage<msdf_atlas::byte, 4>>>;

    export class RETRO_API FontAtlas
    {
        FontAtlas() = default;

      public:
        FontAtlas(const FontAtlas &) = delete;

        FontAtlas(FontAtlas &&other) noexcept;

        ~FontAtlas() = default;

        FontAtlas &operator=(const FontAtlas &) = delete;

        FontAtlas &operator=(FontAtlas &&other) noexcept;

        [[nodiscard]] inline float source_pixel_size() const noexcept
        {
            return source_pixel_size_;
        }

        [[nodiscard]] inline float distance_range() const noexcept
        {
            return distance_range_;
        }

        [[nodiscard]] inline const FontMetrics &metrics() const noexcept
        {
            return metrics_;
        }

        [[nodiscard]] inline const std::unordered_map<char32_t, GlyphMetrics> &glyphs() const noexcept
        {
            std::shared_lock lock{atlas_mutex_};
            return glyphs_;
        }

        [[nodiscard]] inline const RefCountPtr<Texture> &texture() const noexcept
        {
            std::shared_lock lock{atlas_mutex_};
            return texture_;
        }

      private:
        friend FontService;
        friend Font;

        void add_glyphs(RenderBackend &render_backend, msdfgen::FontHandle &handle, std::u32string_view codepoints);

        Task<> add_glyphs_async(RefCountPtr<RenderBackend> render_backend,
                                msdfgen::FontHandle &handle,
                                std::u32string_view codepoints);

        [[nodiscard]] msdf_atlas::Charset get_new_chars(std::u32string_view codepoints) const;

        float source_pixel_size_{64};
        float distance_range_{8.0f};

        FontMetrics metrics_{};
        std::unordered_map<char32_t, GlyphMetrics> glyphs_{};
        RefCountPtr<Texture> texture_{};
        FontAtlasData atlas_{};
        mutable std::shared_mutex atlas_mutex_;
    };

    class FontFace
    {
      public:
        FontFace(FreeTypeLibrary library, std::vector<std::byte> bytes, FreeTypeFace face) noexcept;

      private:
        friend FontService;
        friend Font;

        std::vector<std::byte> bytes_;
        FreeTypeLibrary library_;
        FreeTypeFace face_;
        FontHandlePtr handle_{};
        std::string_view family_name_;
        std::string_view style_name_;
    };

    class Font final : public IntrusiveRefCounted
    {
        explicit Font(RefCountPtr<RenderBackend> render_backend, FontFace font_face, FontAtlas font_atlas) noexcept;

      public:
        [[nodiscard]] inline std::string_view family_name() const noexcept
        {
            return face_.family_name_;
        }

        [[nodiscard]] inline std::string_view style_name() const noexcept
        {
            return face_.style_name_;
        }

        [[nodiscard]] inline const FontAtlas &atlas() const noexcept
        {
            return primary_atlas_;
        }

        void add_glyphs_if_missing(std::u32string_view codepoints);

        Task<> add_glyphs_if_missing_async(std::u32string_view codepoints);

      private:
        friend FontService;

        RefCountPtr<RenderBackend> render_backend_;
        FontFace face_;
        FontAtlas primary_atlas_;
    };

    class RETRO_API FontService : NonCopyable
    {
      public:
        explicit FontService(RenderBackend &render_backend);

        [[nodiscard]] Task<RefCountPtr<Font>> load_font(std::vector<std::byte> bytes) const;

      private:
        Task<FontAtlas> generate_font_atlas(FontFace &face, const FontMsdfAtlasConfig &atlas_config) const;

        RenderBackend &render_backend_;
        FreeTypeLibrary library_{};
    };
} // namespace retro
