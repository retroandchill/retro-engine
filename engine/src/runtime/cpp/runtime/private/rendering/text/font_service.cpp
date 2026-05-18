/**
 * @file font_service.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <ft2build.h>
#include FT_FREETYPE_H

#include <boost/asio/config.hpp>
#include <msdfgen-ext.h>
#include <msdfgen.h>

module retro.runtime.rendering.text.font_service;

import retro.core.util.exceptions;

namespace retro
{
    namespace
    {

        struct FontHandleDeleter
        {
            void operator()(msdfgen::FontHandle *handle) const noexcept
            {
                msdfgen::destroyFont(handle);
            }
        };

        using FontHandlePtr = std::unique_ptr<msdfgen::FontHandle, FontHandleDeleter>;

        FontHandlePtr create_font_handle(FreeTypeFace *face)
        {
            return FontHandlePtr{msdfgen::adoptFreetypeFont(face)};
        }

        void throw_freetype_error(const FT_Error error, std::string_view message)
        {
            if (error != FT_Err_Ok)
            {
                throw PlatformException(std::format("{}: {}", message, FT_Error_String(error)));
            }
        }

        FreeTypeLibraryPtr init_free_type()
        {
            FT_Library library;
            throw_freetype_error(FT_Init_FreeType(&library), "Failed to initialize FreeType library");
            return FreeTypeLibraryPtr{library, FT_Done_FreeType};
        }

        FontAtlas generate_atlas(const FontMsdfAtlasConfig &config,
                                 msdfgen::FontHandle *handle,
                                 const FontFace &face,
                                 RenderBackend &render_backend)
        {

            msdfgen::FontMetrics metrics{};
            msdfgen::getFontMetrics(metrics, handle, msdfgen::FontCoordinateScaling::FONT_SCALING_EM_NORMALIZED);

            FontAtlas output{.size_class = config.size_class,
                             .distance_range = config.distance_range,
                             .metrics = {
                                 .ascender = static_cast<float>(metrics.ascenderY),
                                 .descender = static_cast<float>(metrics.descenderY),
                                 .line_height = static_cast<float>(metrics.lineHeight),
                                 .underline_position = static_cast<float>(metrics.underlineY),
                                 .underline_thickness = static_cast<float>(metrics.underlineThickness),
                             }};

            std::vector<std::byte> source_pixels;
            source_pixels.resize(config.atlas_width * config.atlas_height * 4, std::byte{0});
            std::span pixels = source_pixels;
            std::uint32_t pen_x = 0;
            std::uint32_t pen_y = 0;
            std::uint32_t max_height = 0;

            constexpr std::int32_t atlas_padding = 4;

            for (auto char_code = config.first_codepoint; char_code <= config.last_codepoint; char_code++)
            {
                const auto gindex = face.glyph_index(char_code);
                if (gindex == FontFace::null_glyph_index)
                    continue;

                std::int32_t glyph_width = 0;
                std::int32_t glyph_height = 0;
                double advance = 0;

                msdfgen::Shape shape{};
                msdfgen::Shape::Bounds bounds{};

                msdfgen::loadGlyph(shape, handle, char_code, &advance);

                if (shape.validate() && !shape.contours.empty())
                {
                    shape.normalize();
                    shape.setYAxisOrientation(msdfgen::YAxisOrientation::Y_DOWNWARD);

                    msdfgen::resolveShapeGeometry(shape);

                    bounds = shape.getBounds(config.distance_range);

                    glyph_width = static_cast<std::int32_t>(std::ceil(bounds.r - bounds.l)) + atlas_padding * 2;
                    glyph_height = static_cast<std::int32_t>(std::ceil(bounds.t - bounds.b)) + atlas_padding * 2;

                    if (glyph_height > max_height)
                        max_height = glyph_height;

                    msdfgen::edgeColoringByDistance(shape, 1.0);
                    msdfgen::Bitmap<float, 4> msdf{glyph_width, glyph_height};
                    msdfgen::generateMTSDF(msdf,
                                           shape,
                                           config.distance_range,
                                           1.0,
                                           msdfgen::Vector2(-bounds.l, -bounds.b));

                    if (pen_x + msdf.width() >= config.atlas_width)
                    {
                        pen_x = 0;
                        pen_y += max_height;
                        max_height = 0;
                    }

                    for (std::int32_t row = 0; row < msdf.height(); ++row)
                    {
                        for (std::int32_t col = 0; col < msdf.width(); ++col)
                        {
                            const auto x = pen_x + col + atlas_padding;
                            const auto y = pen_y + row + atlas_padding;

                            const auto index = (y * config.atlas_width + x) * 4;
                            auto pixel = pixels.subspan(index, 4);

                            auto msdf_pixel = std::span{msdf(col, row), 4};

                            pixel[0] = static_cast<std::byte>(msdfgen::pixelFloatToByte(msdf_pixel[0]));
                            pixel[1] = static_cast<std::byte>(msdfgen::pixelFloatToByte(msdf_pixel[1]));
                            pixel[2] = static_cast<std::byte>(msdfgen::pixelFloatToByte(msdf_pixel[2]));
                            pixel[3] = static_cast<std::byte>(msdfgen::pixelFloatToByte(msdf_pixel[3]));
                        }
                    }
                }

                output.glyphs.emplace(
                    static_cast<char32_t>(char_code),
                    GlyphMetrics{
                        .codepoint = char_code,
                        .glyph_index = gindex,
                        .advance_x = static_cast<float>(advance),
                        .bearing_x = static_cast<float>(bounds.l),
                        .bearing_y = static_cast<float>(bounds.t),
                        .width = static_cast<float>(glyph_width),
                        .height = static_cast<float>(glyph_height),
                        .uvs = {
                            .min = {static_cast<float>(pen_x + atlas_padding) / static_cast<float>(config.atlas_width),
                                    static_cast<float>(pen_y + atlas_padding) /
                                        static_cast<float>(config.atlas_height)},
                            .max = {static_cast<float>(pen_x + glyph_width + atlas_padding) /
                                        static_cast<float>(config.atlas_width),
                                    static_cast<float>(pen_y + glyph_height + atlas_padding) /
                                        static_cast<float>(config.atlas_height)},
                        }});

                pen_x += glyph_width;
            }

            output.texture = render_backend.upload_texture(pixels,
                                                           static_cast<std::int32_t>(config.atlas_width),
                                                           static_cast<std::int32_t>(config.atlas_height),
                                                           TextureFormat::unorm,
                                                           TextureFilter::linear);

            return output;
        }
    } // namespace

    // ReSharper disable once CppParameterMayBeConst
    void FreeTypeFaceDeleter::operator()(FT_Face face) const noexcept
    {
        FT_Done_Face(face);
    }

    FontFace::FontFace(FreeTypeLibraryPtr library, std::vector<std::byte> bytes, FreeTypeFacePtr face) noexcept
        : bytes_{std::move(bytes)}, library_{std::move(library)}, face_{std::move(face)},
          family_name_{face_->family_name}, style_name_{face_->style_name}
    {
    }

    std::uint32_t FontFace::glyph_index(const char32_t codepoint) const noexcept
    {
        return FT_Get_Char_Index(face_.get(), codepoint);
    }

    FontMetrics FontFace::metrics(const std::uint32_t pixel_size) const
    {
        auto *face = face_.get();

        if (face == nullptr)
        {
            throw InvalidStateException("Cannot rasterize glyph from a null font face");
        }

        throw_freetype_error(FT_Set_Pixel_Sizes(face, 0, pixel_size), "Failed to set font pixel size");

        return FontMetrics{
            .ascender = static_cast<float>(face->size->metrics.ascender) / 64.0f,
            .descender = static_cast<float>(face->size->metrics.descender) / 64.0f,
            .line_height = static_cast<float>(face->size->metrics.height) / 64.0f,
            .underline_position = static_cast<float>(face->underline_position) / 64.0f,
            .underline_thickness = static_cast<float>(face->underline_thickness) / 64.0f,
        };
    }
    Font::Font(FreeTypeLibraryPtr library,
               std::vector<std::byte> bytes,
               FreeTypeFacePtr face,
               RenderBackend &render_backend) noexcept
        : face_{std::move(library), std::move(bytes), std::move(face)}
    {
        const auto font_handle = create_font_handle(face_.face_.get());
        constexpr FontConfig config{};

        for (auto [i, atlas_config] : config.atlases | std::views::enumerate)
        {
            atlases_[i] = generate_atlas(atlas_config, font_handle.get(), face_, render_backend);
        }
    }

    FontService::FontService(RenderBackend &render_backend)
        : render_backend_{render_backend}, library_{init_free_type()}
    {
    }

    RefCountPtr<Font> FontService::load_font(std::vector<std::byte> bytes) const
    {
        FT_Face face;
        if (const auto error = FT_New_Memory_Face(library_.get(),
                                                  reinterpret_cast<const FT_Byte *>(bytes.data()),
                                                  static_cast<std::int32_t>(bytes.size()),
                                                  0,
                                                  &face);
            error != FT_Err_Ok)
        {
            throw IoException(FT_Error_String(error));
        }

        return RefCountPtr<Font>::ref(new Font{library_, std::move(bytes), FreeTypeFacePtr{face}, render_backend_});
    }
} // namespace retro
