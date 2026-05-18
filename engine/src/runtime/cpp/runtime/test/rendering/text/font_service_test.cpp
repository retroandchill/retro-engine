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
import retro.runtime.rendering.headless_render_backend;

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
    HeadlessRenderBackend render_backend{};
    const FontService service{render_backend};

    auto bytes = read_test_font();
    ASSERT_FALSE(bytes.empty());

    const auto font = service.load_font(std::move(bytes));

    EXPECT_FALSE(font->face().family_name().empty());
    EXPECT_FALSE(font->face().style_name().empty());

    EXPECT_TRUE(font->face().has_glyph(U'A'));
    EXPECT_TRUE(font->face().has_glyph(U'B'));
    EXPECT_TRUE(font->face().has_glyph(U'0'));
    EXPECT_TRUE(font->face().has_glyph(U' '));

    EXPECT_NE(font->face().glyph_index(U'A'), retro::FontFace::null_glyph_index);
}
