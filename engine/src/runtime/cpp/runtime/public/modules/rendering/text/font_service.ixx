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

    export class RETRO_API FontFace
    {
      public:
        static constexpr std::uint32_t null_glyph_index = 0;

      private:
        FontFace(std::vector<std::byte> bytes, FreeTypeFacePtr face) noexcept;

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

        [[nodiscard]] std::uint32_t glyph_index(char32_t codepoint) const noexcept;

      private:
        friend class FontService;

        std::vector<std::byte> bytes_;
        FreeTypeFacePtr face_;
        std::string_view family_name_;
        std::string_view style_name_;
    };

    using FreeTypeLibrary = std::remove_pointer_t<FT_Library>;

    struct FreeTypeLibraryDeleter
    {
        RETRO_API void operator()(FT_Library library) const noexcept;
    };

    using FreeTypeLibraryPtr = std::unique_ptr<FreeTypeLibrary, FreeTypeLibraryDeleter>;

    class RETRO_API FontService : NonCopyable
    {
      public:
        FontService();

        FontFace load_font(std::vector<std::byte> bytes) const;

      private:
        FreeTypeLibraryPtr library_;
    };
} // namespace retro
