/**
 * @file msdf_atlas_gen.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <msdf-atlas-gen/msdf-atlas-gen.h>

export module msdf_atlas;

export namespace msdf_atlas
{
    using msdf_atlas::operator+;
    using msdf_atlas::operator-;
    using msdf_atlas::operator*;
    using msdf_atlas::operator/;

    using msdf_atlas::AtlasStorage;
    using msdf_atlas::BitmapAtlasStorage;
    using msdf_atlas::Charset;
    using msdf_atlas::DynamicAtlas;
    using msdf_atlas::FontGeometry;
    using msdf_atlas::GeneratorAttributes;
    using msdf_atlas::GeneratorFunction;
    using msdf_atlas::GlyphBox;
    using msdf_atlas::GlyphGeometry;
    using msdf_atlas::GridAtlasPacker;
    using msdf_atlas::ImmediateAtlasGenerator;
    using msdf_atlas::OrientedRectangle;
    using msdf_atlas::pad;
    using msdf_atlas::Padding;
    using msdf_atlas::Rectangle;
    using msdf_atlas::RectanglePacker;
    using msdf_atlas::Remap;
    using msdf_atlas::TightAtlasPacker;
    using msdf_atlas::Workload;

#ifndef MSDF_ATLAS_NO_ARTERY_FONT
    using msdf_atlas::ArteryFontExportProperties;
    using msdf_atlas::exportArteryFont;
#endif

    using msdf_atlas::blit;
    using msdf_atlas::exportCSV;

    using msdf_atlas::msdfGenerator;
    using msdf_atlas::mtsdfGenerator;
    using msdf_atlas::psdfGenerator;
    using msdf_atlas::scanlineGenerator;
    using msdf_atlas::sdfGenerator;

    using msdf_atlas::encodePng;
    using msdf_atlas::saveImage;

    using msdf_atlas::JsonAtlasMetrics;

    using msdf_atlas::generateShadronPreview;
    using msdf_atlas::packRectangles;

    using msdf_atlas::PowerOfTwoSizeSelector;
    using msdf_atlas::SquarePowerOfTwoSizeSelector;
    using msdf_atlas::SquareSizeSelector;

    using msdf_atlas::byte;
    using msdf_atlas::unicode_t;

    using msdf_atlas::DimensionsConstraint;
    using msdf_atlas::GlyphIdentifierType;
    using msdf_atlas::ImageFormat;
    using msdf_atlas::ImageType;
    using msdf_atlas::PackingStyle;

    using msdf_atlas::utf8Decode;

    using msdf_atlas::ceilToPOT;
    using msdf_atlas::floorToPOT;
} // namespace msdf_atlas
