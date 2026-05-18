/**
 * @file font.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.text.font_service;

import std;
import retro.core.util.noncopyable;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.texture;
import retro.runtime.rendering.render_backend;
import retro.core.containers.optional;
import retro.runtime.rendering.layout.uvs;

// ReSharper disable CppInconsistentNaming
extern "C"
{
    typedef struct FT_LibraryRec_ *FT_Library;
    typedef struct FT_FaceRec_ *FT_Face;
}
// ReSharper restore CppInconsistentNaming

namespace retro
{
    export class FontService;

    using FreeTypeFace = std::remove_pointer_t<FT_Face>;

    struct FreeTypeFaceDeleter
    {
        RETRO_API void operator()(FT_Face face) const noexcept;
    };

    using FreeTypeFacePtr = std::unique_ptr<FreeTypeFace, FreeTypeFaceDeleter>;

    using FreeTypeLibrary = std::remove_pointer_t<FT_Library>;

    using FreeTypeLibraryPtr = std::shared_ptr<FreeTypeLibrary>;

    export enum class FontSizeClass : std::uint8_t
    {
        small = 0,
        medium = 1,
        large = 2
    };

    export struct FontMsdfAtlasConfig
    {
        FontSizeClass size_class{FontSizeClass::medium};
        std::uint32_t pixel_size{64};
        std::uint32_t atlas_width{1024};
        std::uint32_t atlas_height{1024};
        std::uint32_t padding{8};
        float distance_range{8.0f};
        char32_t first_codepoint{32};
        char32_t last_codepoint{126};
    };

    struct FontConfig
    {
        std::array<FontMsdfAtlasConfig, 3> atlases{
            FontMsdfAtlasConfig{
                .size_class = FontSizeClass::small,
                .pixel_size = 32,
                .atlas_width = 1024,
                .atlas_height = 1024,
                .padding = 6,
                .distance_range = 6.0f,
                .first_codepoint = 32,
                .last_codepoint = 126,
            },
            FontMsdfAtlasConfig{
                .size_class = FontSizeClass::medium,
                .pixel_size = 64,
                .atlas_width = 1024,
                .atlas_height = 1024,
                .padding = 8,
                .distance_range = 8.0f,
                .first_codepoint = 32,
                .last_codepoint = 126,
            },
            FontMsdfAtlasConfig{
                .size_class = FontSizeClass::large,
                .pixel_size = 96,
                .atlas_width = 2048,
                .atlas_height = 2048,
                .padding = 12,
                .distance_range = 12.0f,
                .first_codepoint = 32,
                .last_codepoint = 126,
            },
        };
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
        FontSizeClass size_class{FontSizeClass::medium};

        std::uint32_t source_pixel_size{64};
        float distance_range{8.0f};

        FontMetrics metrics{};
        std::unordered_map<char32_t, GlyphMetrics> glyphs{};
        RefCountPtr<Texture> texture{};
    };

    export class Font;

    export class FontFace : NonCopyable
    {
      public:
        RETRO_API static constexpr std::uint32_t null_glyph_index = 0;

      private:
        FontFace(FreeTypeLibraryPtr library, std::vector<std::byte> bytes, FreeTypeFacePtr face) noexcept;

      public:
        [[nodiscard]] inline std::string_view family_name() const noexcept
        {
            return family_name_;
        }

        [[nodiscard]] inline std::string_view style_name() const noexcept
        {
            return style_name_;
        }

        [[nodiscard]] inline bool has_glyph(const char32_t codepoint) const noexcept
        {
            return glyph_index(codepoint) != null_glyph_index;
        }

        RETRO_API [[nodiscard]] std::uint32_t glyph_index(char32_t codepoint) const noexcept;

        RETRO_API [[nodiscard]] FontMetrics metrics(std::uint32_t pixel_size) const;

      private:
        friend class FontService;
        friend class Font;

        std::vector<std::byte> bytes_;
        FreeTypeLibraryPtr library_;
        FreeTypeFacePtr face_;
        std::string_view family_name_;
        std::string_view style_name_;
    };

    class Font final : public IntrusiveRefCounted
    {
        explicit Font(FreeTypeLibraryPtr library,
                      std::vector<std::byte> bytes,
                      FreeTypeFacePtr face,
                      RenderBackend &render_backend) noexcept;

      public:
        [[nodiscard]] inline const FontFace &face() const noexcept
        {
            return face_;
        }

        [[nodiscard]] inline const FontAtlas &atlas(const FontSizeClass size_class) const noexcept
        {
            return atlases_[static_cast<std::size_t>(size_class)];
        }

        [[nodiscard]] inline const std::array<FontAtlas, 3> &atlases() const noexcept
        {
            return atlases_;
        }

        [[nodiscard]] static constexpr FontSizeClass classify_pixel_size(const std::uint32_t pixel_size) noexcept
        {
            if (pixel_size <= 24)
                return FontSizeClass::small;

            if (pixel_size <= 64)
                return FontSizeClass::medium;

            return FontSizeClass::large;
        }

        [[nodiscard]] inline const FontAtlas &atlas_for_pixel_size(const std::uint32_t pixel_size) const noexcept
        {
            return atlas(classify_pixel_size(pixel_size));
        }

      private:
        friend class FontService;

        FontFace face_;
        std::array<FontAtlas, 3> atlases_;
    };

    class RETRO_API FontService : NonCopyable
    {
      public:
        explicit FontService(RenderBackend &render_backend);

        [[nodiscard]] RefCountPtr<Font> load_font(std::vector<std::byte> bytes) const;

      private:
        RenderBackend &render_backend_;
        FreeTypeLibraryPtr library_;
    };
} // namespace retro
