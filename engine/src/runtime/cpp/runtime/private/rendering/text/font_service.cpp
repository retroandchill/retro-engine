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
            return FreeTypeLibraryPtr{library, FT_Done_FreeType};
        }

        void throw_freetype_error(const FT_Error error, std::string_view message)
        {
            if (error != FT_Err_Ok)
            {
                throw PlatformException(std::format("{}: {}", message, FT_Error_String(error)));
            }
        }

        std::atomic next_font_id{1};

        [[nodiscard]] std::uint32_t generate_font_id() noexcept
        {
            auto next = next_font_id.fetch_add(1, std::memory_order_relaxed);
            if (next == 0)
            {
                next = next_font_id.fetch_add(1, std::memory_order_relaxed);
            }
            return next;
        }

        [[nodiscard]] bool sample_coverage(const RasterizedGlyph &glyph,
                                           const std::int32_t x,
                                           const std::int32_t y) noexcept
        {
            if (x < 0 || y < 0)
            {
                return false;
            }

            const auto ux = static_cast<std::uint32_t>(x);
            const auto uy = static_cast<std::uint32_t>(y);

            if (ux >= glyph.width || uy >= glyph.height)
            {
                return false;
            }

            const auto index = static_cast<std::size_t>(uy) * glyph.width + ux;
            return glyph.coverage[index] > 127;
        }

        [[nodiscard]] float signed_distance_to_glyph(const RasterizedGlyph &glyph,
                                                     const std::int32_t x,
                                                     const std::int32_t y,
                                                     const std::uint32_t padding) noexcept
        {
            const bool inside = sample_coverage(glyph, x, y);
            auto closest_distance_sq = std::numeric_limits<float>::max();

            const auto min_y = -static_cast<std::int32_t>(padding);
            const auto max_y = static_cast<std::int32_t>(glyph.height + padding);
            const auto min_x = -static_cast<std::int32_t>(padding);
            const auto max_x = static_cast<std::int32_t>(glyph.width + padding);

            for (auto oy = min_y; oy < max_y; ++oy)
            {
                for (auto ox = min_x; ox < max_x; ++ox)
                {
                    if (sample_coverage(glyph, ox, oy) == inside)
                    {
                        continue;
                    }

                    const auto dx = static_cast<float>(ox - x);
                    const auto dy = static_cast<float>(oy - y);
                    const auto distance_sq = dx * dx + dy * dy;

                    closest_distance_sq = std::min(closest_distance_sq, distance_sq);
                }
            }

            const auto distance = std::sqrt(closest_distance_sq);
            return inside ? distance : -distance;
        }

        [[nodiscard]] std::byte encode_sdf_value(const float signed_distance, const float spread) noexcept
        {
            const auto normalized = 0.5f + signed_distance / spread;
            const auto clamped = std::clamp(normalized, 0.0f, 1.0f);
            return static_cast<std::byte>(std::round(clamped * 255.0f));
        }

        [[nodiscard]] SdfGlyphBitmap generate_sdf_glyph_bitmap(const RasterizedGlyph &glyph,
                                                               const std::uint32_t padding,
                                                               const float spread)
        {
            SdfGlyphBitmap sdf{.codepoint = glyph.codepoint,
                               .glyph_index = glyph.glyph_index,
                               .advance_x = glyph.advance_x,
                               .bearing_x = glyph.bearing_x,
                               .bearing_y = glyph.bearing_y,
                               .width = glyph.width,
                               .height = glyph.height,
                               .padding = padding};

            sdf.pixels.resize(sdf.width * sdf.height);

            for (std::uint32_t y = 0; y < sdf.height; ++y)
            {
                for (std::uint32_t x = 0; x < sdf.width; ++x)
                {
                    const auto distance = signed_distance_to_glyph(glyph,
                                                                   static_cast<std::int32_t>(x),
                                                                   static_cast<std::int32_t>(y),
                                                                   padding);

                    sdf.pixels[static_cast<std::size_t>(y) * sdf.width + x] = encode_sdf_value(distance, spread);
                }
            }

            return sdf;
        }
    } // namespace

    // ReSharper disable CppParameterMayBeConst
    void FreeTypeFaceDeleter::operator()(FT_Face face) const noexcept
    {
        FT_Done_Face(face);
    }

    FontFace::FontFace(ConstructTag,
                       FreeTypeLibraryPtr library,
                       std::vector<std::byte> bytes,
                       FreeTypeFacePtr face) noexcept
        : id_{generate_font_id()}, bytes_{std::move(bytes)}, library_{std::move(library)}, face_{std::move(face)},
          family_name_{face_->family_name}, style_name_{face_->style_name}
    {
    }

    std::uint32_t FontFace::glyph_index(char32_t codepoint) const noexcept
    {
        return FT_Get_Char_Index(face_.get(), codepoint);
    }

    RasterizedGlyph FontFace::rasterize_glyph(char32_t codepoint, std::uint32_t pixel_size) const
    {
        auto *face = face_.get();

        if (face == nullptr)
        {
            throw InvalidStateException("Cannot rasterize glyph from a null font face");
        }

        throw_freetype_error(FT_Set_Pixel_Sizes(face, 0, pixel_size), "Failed to set font pixel size");

        const auto index = glyph_index(codepoint);
        if (index == null_glyph_index)
        {
            return RasterizedGlyph{
                .codepoint = codepoint,
                .glyph_index = null_glyph_index,
            };
        }

        throw_freetype_error(FT_Load_Glyph(face, index, FT_LOAD_NO_BITMAP), "Failed to load glyph");

        throw_freetype_error(FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL), "Failed to render glyph");

        const auto *slot = face->glyph;
        const auto &bitmap = slot->bitmap;

        RasterizedGlyph glyph{
            .codepoint = codepoint,
            .glyph_index = index,
            .advance_x = static_cast<float>(slot->advance.x) / 64.0f,
            .bearing_x = static_cast<float>(slot->bitmap_left),
            .bearing_y = static_cast<float>(slot->bitmap_top),
            .width = bitmap.width,
            .height = bitmap.rows,
        };

        glyph.coverage.resize(glyph.width * glyph.height);
        for (std::uint32_t y = 0; y < glyph.height; ++y)
        {
            const auto *src_row = std::next(bitmap.buffer, y * bitmap.pitch);
            auto *dst_row = std::next(glyph.coverage.data(), y * glyph.width);

            std::copy_n(src_row, glyph.width, dst_row);
        }

        return glyph;
    }

    FontMetrics FontFace::metrics(std::uint32_t pixel_size) const
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

    FontAtlas FontFace::create_sdf_atlas(const FontSdfConfig &config) const
    {
        if (config.atlas_width == 0 || config.atlas_height == 0)
        {
            throw std::invalid_argument("Atlas dimensions cannot be zero");
        }

        if (config.pixel_size == 0)
        {
            throw std::invalid_argument("Pixel size cannot be zero");
        }

        if (config.spread <= 0.0f)
        {
            throw std::invalid_argument("Spread must be greater than zero");
        }

        FontAtlas atlas{
            .width = config.atlas_width,
            .height = config.atlas_height,
            .metrics = metrics(config.pixel_size),
        };

        atlas.pixels.assign(atlas.width * atlas.height, std::byte{0});

        AtlasPacker packer{
            .width = config.atlas_width,
            .height = config.atlas_height,
        };

        for (auto codepoint = config.first_codepoint; codepoint <= config.last_codepoint; ++codepoint)
        {
            auto rasterized = rasterize_glyph(codepoint, config.pixel_size);

            if (rasterized.glyph_index == FontFace::null_glyph_index)
                continue;

            if (rasterized.width == 0 || rasterized.height == 0)
            {
                atlas.glyphs.emplace(codepoint,
                                     GlyphMetrics{
                                         .codepoint = codepoint,
                                         .glyph_index = rasterized.glyph_index,
                                         .advance_x = rasterized.advance_x,
                                         .bearing_x = rasterized.bearing_x,
                                         .bearing_y = rasterized.bearing_y,
                                         .width = 0.0f,
                                         .height = 0.0f,
                                     });
                continue;
            }

            const auto sdf = generate_sdf_glyph_bitmap(rasterized, config.padding, config.spread);
            const auto position = packer.allocate(sdf.width, sdf.height);

            if (!position.has_value())
            {
                throw PlatformException("Font atlas is too small for the requested glyph range");
            }

            const auto [atlas_x, atlas_y] = *position;
            for (std::uint32_t y = 0; y < sdf.height; ++y)
            {
                const auto src_offset = static_cast<std::size_t>(y) * sdf.width;
                const auto dst_offset = static_cast<std::size_t>(atlas_y + y) * atlas.width + atlas_x;

                std::copy_n(std::next(sdf.pixels.data(), src_offset),
                            sdf.width,
                            std::next(atlas.pixels.data(), dst_offset));
            }

            atlas.glyphs.emplace(
                codepoint,
                GlyphMetrics{
                    .codepoint = codepoint,
                    .glyph_index = sdf.glyph_index,
                    .advance_x = sdf.advance_x,
                    .bearing_x = sdf.bearing_x - static_cast<float>(sdf.padding),
                    .bearing_y = sdf.bearing_y + static_cast<float>(sdf.padding),
                    .width = static_cast<float>(sdf.width),
                    .height = static_cast<float>(sdf.height),
                    .uv_min_x = static_cast<float>(atlas_x) / static_cast<float>(atlas.width),
                    .uv_min_y = static_cast<float>(atlas_y) / static_cast<float>(atlas.height),
                    .uv_max_x = static_cast<float>(atlas_x + sdf.width) / static_cast<float>(atlas.width),
                    .uv_max_y = static_cast<float>(atlas_y + sdf.height) / static_cast<float>(atlas.height),
                });
        }

        return atlas;
    }
    // ReSharper restore CppParameterMayBeConst

    FontService::FontService() : library_{init_free_type()}
    {
    }

    RefCountPtr<FontFace> FontService::load_font(std::vector<std::byte> bytes) const
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

        return make_ref_counted<FontFace>(FontFace::ConstructTag{}, library_, std::move(bytes), FreeTypeFacePtr{face});
    }
} // namespace retro
