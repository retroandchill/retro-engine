/**
 * @file font_dependencies.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <ft2build.h>
#include FT_FREETYPE_H
#include FT_MODULE_H

#include <msdf-atlas-gen/msdf-atlas-gen.h>
#include <msdfgen-ext.h>
#include <msdfgen.h>

export module retro.runtime.rendering.text.font:dependencies;

export namespace msdfgen
{
    using msdfgen::Bitmap;
    using msdfgen::BitmapConstRef;
    using msdfgen::BitmapConstSection;
    using msdfgen::BitmapRef;
    using msdfgen::BitmapSection;
    using msdfgen::byte;

    using msdfgen::adoptFreetypeFont;
    using msdfgen::destroyFont;
} // namespace msdfgen

export namespace msdf_atlas
{
    using msdf_atlas::BitmapAtlasStorage;
    using msdf_atlas::byte;
    using msdf_atlas::ceilToPOT;
    using msdf_atlas::Charset;
    using msdf_atlas::FontGeometry;
    using msdf_atlas::GeneratorAttributes;
    using msdf_atlas::GeneratorFunction;
    using msdf_atlas::GlyphBox;
    using msdf_atlas::GlyphGeometry;
    using msdf_atlas::mtsdfGenerator;
    using msdf_atlas::RectanglePacker;
    using msdf_atlas::Remap;
    using msdf_atlas::TightAtlasPacker;
} // namespace msdf_atlas

export using ::FT_Error;
export using ::FT_Err_Ok;
export using ::FT_Error_String;
export using ::FT_Done_Face;
export using ::FT_Reference_Face;
export using ::FT_Done_FreeType;
export using ::FT_New_Memory_Face;
export using ::FT_Init_FreeType;
export using ::FT_Reference_Library;
