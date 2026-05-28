/**
 * @file msdfgen.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <msdfgen-ext.h>
#include <msdfgen.h>

export module msdfgen;

export namespace msdfgen
{
    using msdfgen::operator==;
    using msdfgen::operator!=;
    using msdfgen::operator<;
    using msdfgen::operator>;
    using msdfgen::operator<=;
    using msdfgen::operator>=;

    using msdfgen::operator+;
    using msdfgen::operator-;
    using msdfgen::operator*;
    using msdfgen::operator/;
    using msdfgen::operator!;

    using msdfgen::Bitmap;
    using msdfgen::BitmapConstRef;
    using msdfgen::BitmapConstSection;
    using msdfgen::BitmapRef;
    using msdfgen::BitmapSection;
    using msdfgen::Contour;
    using msdfgen::DistanceMapping;
    using msdfgen::EdgeColor;
    using msdfgen::EdgeHolder;
    using msdfgen::Projection;
    using msdfgen::Range;
    using msdfgen::SDFTransformation;

    using msdfgen::FillRule;
    using msdfgen::interpretFillRule;
    using msdfgen::Scanline;
    constexpr inline auto CORNER_DOT_EPSILON = MSDFGEN_CORNER_DOT_EPSILON;
    using msdfgen::crossProduct;
    using msdfgen::dotProduct;
    using msdfgen::Point2;
    using msdfgen::Shape;
    using msdfgen::SignedDistance;
    using msdfgen::Vector2;
    using msdfgen::YAxisOrientation;

    constexpr inline auto Y_AXIS_DEFAULT_ORIENTATION = MSDFGEN_Y_AXIS_DEFAULT_ORIENTATION;
    constexpr inline auto Y_AXIS_NONDEFAULT_ORIENTATION = MSDFGEN_Y_AXIS_NONDEFAULT_ORIENTATION;

    using msdfgen::clamp;
    using msdfgen::max;
    using msdfgen::median;
    using msdfgen::min;
    using msdfgen::mix;
    using msdfgen::nonZeroSign;
    using msdfgen::sign;

    using msdfgen::byte;

    using msdfgen::interpolate;

    using msdfgen::edgeColoringByDistance;
    using msdfgen::edgeColoringInkTrap;
    using msdfgen::edgeColoringSimple;

    constexpr inline auto CUBIC_SEARCH_STARTS = MSDFGEN_CUBIC_SEARCH_STARTS;
    constexpr inline auto CUBIC_SEARCH_STEPS = MSDFGEN_CUBIC_SEARCH_STEPS;
    using msdfgen::CubicSegment;
    using msdfgen::EdgeSegment;
    using msdfgen::LinearSegment;
    using msdfgen::QuadraticSegment;

    using msdfgen::saveSvgShape;

    using msdfgen::ErrorCorrectionConfig;
    using msdfgen::GeneratorConfig;
    using msdfgen::MSDFGeneratorConfig;

    using msdfgen::msdfErrorCorrection;
    using msdfgen::msdfFastDistanceErrorCorrection;
    using msdfgen::msdfFastEdgeErrorCorrection;

    using msdfgen::pixelByteToFloat;
    using msdfgen::pixelFloatToByte;

    using msdfgen::distanceSignCorrection;
    using msdfgen::rasterize;

    using msdfgen::renderSDF;
    using msdfgen::simulate8bit;

    using msdfgen::saveBmp;
    using msdfgen::saveFl32;
    using msdfgen::saveRgba;
    using msdfgen::saveTiff;

    using msdfgen::estimateSDFError;
    using msdfgen::scanlineSDF;

    using msdfgen::readShapeDescription;
    using msdfgen::writeShapeDescription;

    using msdfgen::unicode_t;

    using msdfgen::deinitializeFreetype;
    using msdfgen::FontCoordinateScaling;
    using msdfgen::FontHandle;
    using msdfgen::FontMetrics;
    using msdfgen::FontVariationAxis;
    using msdfgen::FreetypeHandle;
    using msdfgen::GlyphIndex;
    using msdfgen::initializeFreetype;

    using msdfgen::destroyFont;
    using msdfgen::getFontMetrics;
    using msdfgen::getFontWhitespaceWidth;
    using msdfgen::getGlyphCount;
    using msdfgen::getGlyphIndex;
    using msdfgen::getKerning;
    using msdfgen::listFontVariationAxes;
    using msdfgen::loadFont;
    using msdfgen::loadFontData;
    using msdfgen::loadGlyph;
    using msdfgen::setFontVariationAxis;

    using msdfgen::SVG_IMPORT_FAILURE;
    using msdfgen::SVG_IMPORT_INCOMPLETE_FLAG;
    using msdfgen::SVG_IMPORT_PARTIAL_FAILURE_FLAG;
    using msdfgen::SVG_IMPORT_SUCCESS_FLAG;
    using msdfgen::SVG_IMPORT_TRANSFORMATION_IGNORED_FLAG;
    using msdfgen::SVG_IMPORT_UNSUPPORTED_FEATURE_FLAG;

    using msdfgen::buildShapeFromSvgPath;
    using msdfgen::loadSvgShape;

    using msdfgen::resolveShapeGeometry;

    using msdfgen::savePng;
} // namespace msdfgen
