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

namespace retro
{
    export class FontService;

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

    export struct FontAtlas
    {
        float source_pixel_size{64};
        float distance_range{8.0f};

        FontMetrics metrics{};
        std::unordered_map<char32_t, GlyphMetrics> glyphs{};
        RefCountPtr<Texture> texture{};
    };

    export class Font;

    export class FontFace
    {
      public:
        RETRO_API static constexpr std::uint32_t null_glyph_index = 0;

      private:
        FontFace(FreeTypeLibrary library, std::vector<std::byte> bytes, FreeTypeFace face) noexcept;

      public:
        [[nodiscard]] inline std::string_view family_name() const noexcept
        {
            return family_name_;
        }

        [[nodiscard]] inline std::string_view style_name() const noexcept
        {
            return style_name_;
        }

      private:
        friend class FontService;
        friend class Font;

        std::vector<std::byte> bytes_;
        FreeTypeLibrary library_;
        FreeTypeFace face_;
        std::string_view family_name_;
        std::string_view style_name_;
    };

    class Font final : public IntrusiveRefCounted
    {
        explicit Font(FontFace font_face, FontAtlas font_atlas) noexcept;

      public:
        [[nodiscard]] inline const FontFace &face() const noexcept
        {
            return face_;
        }

        [[nodiscard]] inline const FontAtlas &atlas() const noexcept
        {
            return primary_atlas_;
        }

      private:
        friend class FontService;

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
