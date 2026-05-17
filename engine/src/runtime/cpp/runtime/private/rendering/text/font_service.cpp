/**
 * @file font.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <ft2build.h>
#include FT_FREETYPE_H

module retro.runtime.rendering.text.font_service;

import retro.core.util.exceptions;

namespace retro
{
    namespace
    {
        FreeTypeLibraryPtr init_free_type()
        {
            FT_Library library;
            if (const auto error = FT_Init_FreeType(&library); error != FT_Err_Ok)
            {
                throw PlatformException(FT_Error_String(error));
            }
            return FreeTypeLibraryPtr{library};
        }
    } // namespace

    // ReSharper disable CppParameterMayBeConst
    void FreeTypeFaceDeleter::operator()(FT_Face face) const noexcept
    {
        FT_Done_Face(face);
    }

    FontFace::FontFace(std::vector<std::byte> bytes, FreeTypeFacePtr face) noexcept
        : bytes_{std::move(bytes)}, face_{std::move(face)}, family_name_{face_->family_name},
          style_name_{face_->style_name}
    {
    }

    std::uint32_t FontFace::glyph_index(char32_t codepoint) const noexcept
    {
        return FT_Get_Char_Index(face_.get(), codepoint);
    }

    void FreeTypeLibraryDeleter::operator()(FT_Library library) const noexcept
    {
        FT_Done_FreeType(library);
    }
    // ReSharper restore CppParameterMayBeConst

    FontService::FontService() : library_{init_free_type()}
    {
    }

    FontFace FontService::load_font(std::vector<std::byte> bytes) const
    {
        FT_Face face;
        if (const auto error = FT_New_Memory_Face(library_.get(),
                                                  reinterpret_cast<const FT_Byte *>(bytes.data()),
                                                  bytes.size(),
                                                  0,
                                                  &face);
            error != FT_Err_Ok)
        {
            throw IoException(FT_Error_String(error));
        }

        return FontFace{std::move(bytes), FreeTypeFacePtr{face}};
    }
} // namespace retro
