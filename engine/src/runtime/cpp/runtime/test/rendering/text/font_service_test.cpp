/**
 * @file font_service_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.runtime.rendering.text.font_service;
import std;
import retro.core.io.file_stream;
import retro.runtime.rendering.text.font;

using namespace retro;

namespace
{
    [[nodiscard]] std::filesystem::path test_data_dir()
    {
        return std::filesystem::current_path();
    }

    [[nodiscard]] std::vector<std::byte> read_binary_file(const std::filesystem::path &path)
    {
        auto bytes = FileStream::open(path, FileOpenMode::read_only)
                         .and_then([](const std::unique_ptr<FileStream> &stream) { return stream->read_all(); });

        if (!bytes.has_value())
        {
            throw std::runtime_error{std::format("Failed to read test file: {}", path.string())};
        }

        return *std::move(bytes);
    }

    [[nodiscard]] std::vector<std::byte> read_test_font()
    {
        return read_binary_file(test_data_dir() / "resources" / "test_font.ttf");
    }
} // namespace

TEST(FontService, LoadFontFace)
{
    const FontService service;

    auto bytes = read_test_font();
    ASSERT_FALSE(bytes.empty());

    const auto font = service.load_font(std::move(bytes));

    EXPECT_FALSE(font.family_name().empty());
    EXPECT_FALSE(font.style_name().empty());

    EXPECT_TRUE(font.has_glyph(U'A'));
    EXPECT_TRUE(font.has_glyph(U'B'));
    EXPECT_TRUE(font.has_glyph(U'0'));
    EXPECT_TRUE(font.has_glyph(U' '));

    EXPECT_NE(font.glyph_index(U'A'), retro::FontFace::null_glyph_index);
}

TEST(FontService, RasterizeGlyphFromLoadedFont)
{
    const FontService service;

    const auto font = service.load_font(read_test_font());
    auto glyph = font.rasterize_glyph(U'A', 48);

    EXPECT_EQ(glyph.codepoint, U'A');
    EXPECT_NE(glyph.glyph_index, retro::FontFace::null_glyph_index);

    EXPECT_GT(glyph.advance_x, 0.0f);
    EXPECT_GT(glyph.width, 0u);
    EXPECT_GT(glyph.height, 0u);

    ASSERT_EQ(glyph.coverage.size(), static_cast<std::size_t>(glyph.width) * glyph.height);

    const auto has_non_empty_pixel =
        std::ranges::any_of(glyph.coverage, [](const std::uint8_t value) { return value != 0; });

    EXPECT_TRUE(has_non_empty_pixel);
}

TEST(FontService, RasterizeSpaceGlyphHasAdvanceButMayHaveNoPixels)
{
    const FontService service;

    const auto font = service.load_font(read_test_font());
    const auto glyph = font.rasterize_glyph(U' ', 48);

    EXPECT_EQ(glyph.codepoint, U' ');
    EXPECT_NE(glyph.glyph_index, retro::FontFace::null_glyph_index);

    EXPECT_GT(glyph.advance_x, 0.0f);

    EXPECT_EQ(glyph.width, 0u);
    EXPECT_EQ(glyph.height, 0u);
    EXPECT_TRUE(glyph.coverage.empty());
}

TEST(FontService, CreateSdfAtlasForAsciiRange)
{
    FontService service;

    auto font = service.load_font(read_test_font());

    const auto atlas = service.create_sdf_atlas(font,
                                                FontSdfConfig{
                                                    .pixel_size = 48,
                                                    .atlas_width = 512,
                                                    .atlas_height = 512,
                                                    .padding = 8,
                                                    .spread = 8.0f,
                                                    .first_codepoint = 32,
                                                    .last_codepoint = 126,
                                                });

    EXPECT_EQ(atlas.width, 512u);
    EXPECT_EQ(atlas.height, 512u);
    EXPECT_EQ(atlas.pixels.size(), 512u * 512u);

    EXPECT_TRUE(atlas.glyphs.contains(U'A'));
    EXPECT_TRUE(atlas.glyphs.contains(U'B'));
    EXPECT_TRUE(atlas.glyphs.contains(U'0'));
    EXPECT_TRUE(atlas.glyphs.contains(U' '));

    const auto &a = atlas.glyphs.at(U'A');

    EXPECT_NE(a.glyph_index, retro::FontFace::null_glyph_index);
    EXPECT_GT(a.advance_x, 0.0f);
    EXPECT_GT(a.width, 0.0f);
    EXPECT_GT(a.height, 0.0f);

    EXPECT_GE(a.uv_min_x, 0.0f);
    EXPECT_GE(a.uv_min_y, 0.0f);
    EXPECT_LE(a.uv_max_x, 1.0f);
    EXPECT_LE(a.uv_max_y, 1.0f);
    EXPECT_LT(a.uv_min_x, a.uv_max_x);
    EXPECT_LT(a.uv_min_y, a.uv_max_y);

    const auto has_non_empty_pixel =
        std::ranges::any_of(atlas.pixels, [](const std::uint8_t value) { return value != 0; });

    EXPECT_TRUE(has_non_empty_pixel);
}
