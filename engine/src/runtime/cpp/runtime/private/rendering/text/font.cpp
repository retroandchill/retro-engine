/**
 * @file font.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.text.font;

import retro.core.util.exceptions;
import :atlas_generator;

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

        FontHandlePtr create_font_handle(FT_Face face)
        {
            return FontHandlePtr{msdfgen::adoptFreetypeFont(face)};
        }
    } // namespace

    FontFace::FontFace(FreeTypeLibrary library, std::vector<std::byte> bytes, FreeTypeFace face) noexcept
        : bytes_{std::move(bytes)}, library_{std::move(library)}, face_{std::move(face)},
          family_name_{face_->family_name}, style_name_{face_->style_name}
    {
    }

    Font::Font(FontFace font_face, FontAtlas font_atlas) noexcept
        : face_{std::move(font_face)}, primary_atlas_{std::move(font_atlas)}
    {
    }

    FontService::FontService(RenderBackend &render_backend) : render_backend_{render_backend}
    {
    }

    Task<RefCountPtr<Font>> FontService::load_font(std::vector<std::byte> bytes) const
    {
        FontFace font_face{library_, std::move(bytes), library_.load_face(bytes)};

        constexpr FontMsdfAtlasConfig atlas_config{.pixel_size = 24, .distance_range = 2.0f};

        auto atlas = co_await generate_font_atlas(font_face, atlas_config);

        co_return RefCountPtr<Font>::ref(new Font{std::move(font_face), std::move(atlas)});
    }

    Task<FontAtlas> FontService::generate_font_atlas(FontFace &face, const FontMsdfAtlasConfig &atlas_config) const
    {
        const auto handle = create_font_handle(face.face_.get());

        std::vector<msdf_atlas::GlyphGeometry> glyphs;
        msdf_atlas::FontGeometry font_geometry{&glyphs};
        font_geometry.loadCharset(handle.get(), 1.0, msdf_atlas::Charset::ASCII);

        auto metrics = font_geometry.getMetrics();

        FontAtlas output{.distance_range = atlas_config.distance_range,
                         .metrics = {
                             .ascender = static_cast<float>(metrics.ascenderY),
                             .descender = static_cast<float>(metrics.descenderY),
                             .line_height = static_cast<float>(metrics.lineHeight),
                             .underline_position = static_cast<float>(metrics.underlineY),
                             .underline_thickness = static_cast<float>(metrics.underlineThickness),
                         }};

        msdf_atlas::TightAtlasPacker packer;
        packer.setDimensionsConstraint(msdf_atlas::DimensionsConstraint::SQUARE);
        packer.setMinimumScale(atlas_config.pixel_size);
        packer.setPixelRange(atlas_config.distance_range);
        packer.setMiterLimit(1.0);
        packer.pack(glyphs.data(), static_cast<std::int32_t>(glyphs.size()));
        std::int32_t width;
        std::int32_t height;
        packer.getDimensions(width, height);
        AtlasGenerator<float, 4, msdf_atlas::mtsdfGenerator, msdf_atlas::BitmapAtlasStorage<msdf_atlas::byte, 4>>
            generator{width, height};

        msdf_atlas::GeneratorAttributes attributes;
        generator.set_attributes(attributes);
        generator.set_thread_count(4);
        co_await generator.generate_async(glyphs);

        output.glyphs.reserve(glyphs.size());
        for (const auto &glyph : glyphs)
        {

            auto code_point = static_cast<char32_t>(glyph.getCodepoint());
            msdfgen::Shape::Bounds bounds{};
            glyph.getQuadAtlasBounds(bounds.l, bounds.b, bounds.r, bounds.t);
            auto total_width = bounds.r - bounds.l;
            auto total_height = bounds.t - bounds.b;
            msdfgen::Shape::Bounds bearing_bounds{};
            glyph.getQuadPlaneBounds(bearing_bounds.l, bearing_bounds.b, bearing_bounds.r, bearing_bounds.t);
            output.glyphs.emplace(
                glyph.getCodepoint(),
                GlyphMetrics{.codepoint = code_point,
                             .glyph_index = glyph.getGlyphIndex().getIndex(),
                             .advance_x = static_cast<float>(glyph.getAdvance() * packer.getScale()),
                             .bearing_x = static_cast<float>(bearing_bounds.l * packer.getScale()),
                             .bearing_y = static_cast<float>(bearing_bounds.t * packer.getScale()),
                             .width = static_cast<float>(total_width),
                             .height = static_cast<float>(total_height),
                             .uvs = {.min = {static_cast<float>(bounds.l) / static_cast<float>(width),
                                             static_cast<float>(bounds.t) / static_cast<float>(height)},
                                     .max = {static_cast<float>(bounds.r) / static_cast<float>(width),
                                             static_cast<float>(bounds.b) / static_cast<float>(height)}}});
        }

        msdfgen::BitmapConstSection<msdf_atlas::byte, 4> section = generator.atlas_storage();

        std::span pixels{reinterpret_cast<const std::byte *>(section.pixels),
                         static_cast<std::size_t>(section.width * section.height * 4)};

        output.source_pixel_size = static_cast<float>(packer.getScale());
        output.texture = co_await render_backend_.upload_texture(pixels,
                                                                 section.width,
                                                                 section.height,
                                                                 TextureFormat::unorm,
                                                                 TextureFilter::linear);

        co_return output;
    }
} // namespace retro
