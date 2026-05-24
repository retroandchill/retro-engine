/**
 * @file font.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.text.font;

import retro.core.util.exceptions;
import :async_atlas_generator;

namespace retro
{
    FontAtlas::FontAtlas(FontAtlas &&other) noexcept
        : source_pixel_size_{other.source_pixel_size_}, distance_range_{other.distance_range_},
          metrics_{other.metrics_}, glyphs_{std::move(other.glyphs_)}, texture_{std::move(other.texture_)},
          atlas_{std::move(other.atlas_)}
    {
    }

    FontAtlas &FontAtlas::operator=(FontAtlas &&other) noexcept
    {
        if (this != &other)
        {
            source_pixel_size_ = other.source_pixel_size_;
            distance_range_ = other.distance_range_;
            metrics_ = other.metrics_;
            glyphs_ = std::move(other.glyphs_);
            texture_ = std::move(other.texture_);
            atlas_ = std::move(other.atlas_);
        }
        return *this;
    }

    void FontAtlas::add_glyphs(RenderBackend &render_backend,
                               msdfgen::FontHandle &handle,
                               std::u32string_view codepoints)
    {
        SemaphoreGuard guard{atlas_semaphore_};
        auto new_chars = get_new_chars(codepoints);
        if (new_chars.empty())
            return;

        std::vector<msdf_atlas::GlyphGeometry> glyphs;
        msdf_atlas::FontGeometry font_geometry{&glyphs};
        font_geometry.loadCharset(&handle, 1.0, new_chars);

        msdf_atlas::TightAtlasPacker packer;
        packer.setDimensionsConstraint(msdf_atlas::DimensionsConstraint::SQUARE);
        packer.setScale(source_pixel_size_);
        packer.setPixelRange(distance_range_);
        packer.setMiterLimit(1.0);
        packer.pack(glyphs.data(), static_cast<std::int32_t>(glyphs.size()));

        msdfgen::BitmapConstRef<msdfgen::byte, 4> old_storage = atlas_.atlas_generator().atlas_storage();
        auto old_width = old_storage.width;
        auto old_height = old_storage.height;

        auto result = atlas_.add(glyphs);

        msdfgen::BitmapConstRef<msdfgen::byte, 4> storage = atlas_.atlas_generator().atlas_storage();
        auto width = storage.width;
        auto height = storage.height;

        if (has_any_flags(result, AtlasChangeFlag::resized))
        {
            auto width_ratio = static_cast<float>(old_width) / static_cast<float>(width);
            auto height_ratio = static_cast<float>(old_height) / static_cast<float>(height);
            for (auto &glyph_metrics : glyphs_ | std::views::values)
            {
                glyph_metrics.uvs.min.x *= width_ratio;
                glyph_metrics.uvs.min.y *= height_ratio;
                glyph_metrics.uvs.max.x *= width_ratio;
                glyph_metrics.uvs.max.y *= height_ratio;
            }
        }

        for (const auto &glyph : glyphs)
        {
            auto code_point = static_cast<char32_t>(glyph.getCodepoint());
            msdfgen::Shape::Bounds bounds{};
            glyph.getQuadAtlasBounds(bounds.l, bounds.b, bounds.r, bounds.t);
            auto total_width = bounds.r - bounds.l;
            auto total_height = bounds.t - bounds.b;
            msdfgen::Shape::Bounds bearing_bounds{};
            glyph.getQuadPlaneBounds(bearing_bounds.l, bearing_bounds.b, bearing_bounds.r, bearing_bounds.t);
            glyphs_.emplace(glyph.getCodepoint(),
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

        std::span pixels{reinterpret_cast<const std::byte *>(storage.pixels),
                         static_cast<std::size_t>(storage.width * storage.height * 4)};

        texture_ =
            render_backend
                .upload_texture(pixels, storage.width, storage.height, TextureFormat::unorm, TextureFilter::linear)
                .configure_await(false)
                .get();
    }

    Task<> FontAtlas::add_glyphs_async(RefCountPtr<RenderBackend> render_backend,
                                       msdfgen::FontHandle &handle,
                                       std::u32string_view codepoints)
    {
        auto guard = co_await atlas_semaphore_.enter_scope_async();
        auto new_chars = get_new_chars(codepoints);
        if (new_chars.empty())
            co_return;

        std::vector<msdf_atlas::GlyphGeometry> glyphs;
        msdf_atlas::FontGeometry font_geometry{&glyphs};
        font_geometry.loadCharset(&handle, 1.0, new_chars);

        msdf_atlas::TightAtlasPacker packer;
        packer.setDimensionsConstraint(msdf_atlas::DimensionsConstraint::SQUARE);
        packer.setScale(source_pixel_size_);
        packer.setPixelRange(distance_range_);
        packer.setMiterLimit(1.0);
        packer.pack(glyphs.data(), static_cast<std::int32_t>(glyphs.size()));

        msdfgen::BitmapConstRef<msdfgen::byte, 4> old_storage = atlas_.atlas_generator().atlas_storage();
        auto old_width = old_storage.width;
        auto old_height = old_storage.height;

        auto result = co_await atlas_.add_async(glyphs).configure_await(false);

        msdfgen::BitmapConstRef<msdfgen::byte, 4> storage = atlas_.atlas_generator().atlas_storage();
        auto width = storage.width;
        auto height = storage.height;

        if (has_any_flags(result, AtlasChangeFlag::resized))
        {
            auto width_ratio = static_cast<float>(old_width) / static_cast<float>(width);
            auto height_ratio = static_cast<float>(old_height) / static_cast<float>(height);
            for (auto &glyph_metrics : glyphs_ | std::views::values)
            {
                glyph_metrics.uvs.min.x *= width_ratio;
                glyph_metrics.uvs.min.y *= height_ratio;
                glyph_metrics.uvs.max.x *= width_ratio;
                glyph_metrics.uvs.max.y *= height_ratio;
            }
        }

        for (const auto &glyph : glyphs)
        {
            auto code_point = static_cast<char32_t>(glyph.getCodepoint());
            msdfgen::Shape::Bounds bounds{};
            glyph.getQuadAtlasBounds(bounds.l, bounds.b, bounds.r, bounds.t);
            auto total_width = bounds.r - bounds.l;
            auto total_height = bounds.t - bounds.b;
            msdfgen::Shape::Bounds bearing_bounds{};
            glyph.getQuadPlaneBounds(bearing_bounds.l, bearing_bounds.b, bearing_bounds.r, bearing_bounds.t);
            glyphs_.emplace(glyph.getCodepoint(),
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

        std::span pixels{reinterpret_cast<const std::byte *>(storage.pixels),
                         static_cast<std::size_t>(storage.width * storage.height * 4)};

        texture_ =
            co_await render_backend
                ->upload_texture(pixels, storage.width, storage.height, TextureFormat::unorm, TextureFilter::linear)
                .configure_await(false);
    }

    msdf_atlas::Charset FontAtlas::get_new_chars(std::u32string_view codepoints) const
    {
        msdf_atlas::Charset result;
        for (const auto codepoint :
             codepoints | std::views::filter([this](const char32_t c) { return !glyphs_.contains(c); }))
        {
            result.add(codepoint);
        }

        return result;
    }

    FontFace::FontFace(FreeTypeLibrary library, std::vector<std::byte> bytes, FreeTypeFace face) noexcept
        : bytes_{std::move(bytes)}, library_{std::move(library)}, face_{std::move(face)},
          handle_{create_font_handle(face_.get())}, family_name_{face_->family_name}, style_name_{face_->style_name}
    {
    }

    Font::Font(RefCountPtr<RenderBackend> render_backend, FontFace font_face, FontAtlas font_atlas) noexcept
        : render_backend_{std::move(render_backend)}, face_{std::move(font_face)}, primary_atlas_{std::move(font_atlas)}
    {
    }
    void Font::add_glyphs_if_missing(const std::u32string_view codepoints)
    {
        primary_atlas_.add_glyphs(*render_backend_, *face_.handle_, codepoints);
    }

    Task<> Font::add_glyphs_if_missing_async(const std::u32string_view codepoints)
    {
        return primary_atlas_.add_glyphs_async(render_backend_, *face_.handle_, codepoints);
    }

    FontService::FontService(RenderBackend &render_backend) : render_backend_{render_backend}
    {
    }

    Task<RefCountPtr<Font>> FontService::load_font(std::vector<std::byte> bytes) const
    {
        FontFace font_face{library_, std::move(bytes), library_.load_face(bytes)};

        constexpr FontMsdfAtlasConfig atlas_config{.pixel_size = 24, .distance_range = 2.0f};

        auto atlas = co_await generate_font_atlas(font_face, atlas_config);

        co_return RefCountPtr<Font>::ref(
            new Font{render_backend_.shared_from_this(), std::move(font_face), std::move(atlas)});
    }

    Task<FontAtlas> FontService::generate_font_atlas(FontFace &face, const FontMsdfAtlasConfig &atlas_config) const
    {
        FontAtlas output{};
        std::vector<msdf_atlas::GlyphGeometry> glyphs;
        msdf_atlas::FontGeometry font_geometry{&glyphs};
        font_geometry.loadCharset(face.handle_.get(), 1.0, msdf_atlas::Charset::ASCII);

        auto metrics = font_geometry.getMetrics();

        output.distance_range_ = atlas_config.distance_range;
        output.metrics_ = {
            .ascender = static_cast<float>(metrics.ascenderY),
            .descender = static_cast<float>(metrics.descenderY),
            .line_height = static_cast<float>(metrics.lineHeight),
            .underline_position = static_cast<float>(metrics.underlineY),
            .underline_thickness = static_cast<float>(metrics.underlineThickness),
        };

        msdf_atlas::TightAtlasPacker packer;
        packer.setDimensionsConstraint(msdf_atlas::DimensionsConstraint::SQUARE);
        packer.setMinimumScale(atlas_config.pixel_size);
        packer.setPixelRange(atlas_config.distance_range);
        packer.setMiterLimit(1.0);
        packer.pack(glyphs.data(), static_cast<std::int32_t>(glyphs.size()));
        auto &generator = output.atlas_.atlas_generator();

        msdf_atlas::GeneratorAttributes attributes;
        generator.set_attributes(attributes);
        generator.set_thread_count(4);
        co_await output.atlas_.add_async(glyphs);

        msdfgen::BitmapConstRef<msdfgen::byte, 4> storage = output.atlas_.atlas_generator().atlas_storage();
        auto width = storage.width;
        auto height = storage.height;

        output.glyphs_.reserve(glyphs.size());
        for (const auto &glyph : glyphs)
        {
            auto code_point = static_cast<char32_t>(glyph.getCodepoint());
            msdfgen::Shape::Bounds bounds{};
            glyph.getQuadAtlasBounds(bounds.l, bounds.b, bounds.r, bounds.t);
            auto total_width = bounds.r - bounds.l;
            auto total_height = bounds.t - bounds.b;
            msdfgen::Shape::Bounds bearing_bounds{};
            glyph.getQuadPlaneBounds(bearing_bounds.l, bearing_bounds.b, bearing_bounds.r, bearing_bounds.t);
            output.glyphs_.emplace(
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

        output.source_pixel_size_ = static_cast<float>(packer.getScale());
        output.texture_ =
            co_await render_backend_
                .upload_texture(pixels, section.width, section.height, TextureFormat::unorm, TextureFilter::linear)
                .configure_await(false);

        co_return output;
    }
} // namespace retro
