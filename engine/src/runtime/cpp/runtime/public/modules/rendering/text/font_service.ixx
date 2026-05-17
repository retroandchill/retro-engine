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
import retro.runtime.rendering.text.font;
import retro.core.memory.ref_counted_ptr;

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

    export class FontFace final : public IntrusiveRefCounted
    {
      public:
        RETRO_API static constexpr std::uint32_t null_glyph_index = 0;

      private:
        struct ConstructTag
        {
        };

      public:
        RETRO_API FontFace(ConstructTag,
                           FreeTypeLibraryPtr library,
                           std::vector<std::byte> bytes,
                           FreeTypeFacePtr face) noexcept;

        [[nodiscard]] inline std::uint64_t id() const noexcept
        {
            return id_;
        }

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

        RETRO_API RasterizedGlyph rasterize_glyph(char32_t codepoint, std::uint32_t pixel_size) const;

        RETRO_API [[nodiscard]] FontMetrics metrics(std::uint32_t pixel_size) const;

        RETRO_API FontAtlas create_sdf_atlas(const FontSdfConfig &config) const;

      private:
        friend class FontService;

        std::uint64_t id_;
        std::vector<std::byte> bytes_;
        FreeTypeLibraryPtr library_;
        FreeTypeFacePtr face_;
        std::string_view family_name_;
        std::string_view style_name_;
    };

    class RETRO_API FontService : NonCopyable
    {
      public:
        FontService();

        [[nodiscard]] RefCountPtr<FontFace> load_font(std::vector<std::byte> bytes) const;

      private:
        FreeTypeLibraryPtr library_;
    };
} // namespace retro
