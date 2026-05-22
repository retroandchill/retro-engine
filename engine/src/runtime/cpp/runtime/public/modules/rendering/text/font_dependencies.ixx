/**
 * @file font_dependencies.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <ft2build.h>
#include FT_FREETYPE_H

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
} // namespace msdfgen

export namespace msdf_atlas
{
    using msdf_atlas::BitmapAtlasStorage;
    using msdf_atlas::GeneratorAttributes;
    using msdf_atlas::GeneratorFunction;
    using msdf_atlas::GlyphBox;
    using msdf_atlas::GlyphGeometry;
    using msdf_atlas::Remap;
} // namespace msdf_atlas
