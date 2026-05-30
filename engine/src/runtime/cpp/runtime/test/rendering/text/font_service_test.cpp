/**
 * @file font_service_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.runtime.rendering.text.font;
import std;
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
        if (std::ifstream file(path, std::ios::binary); file.is_open())
        {
            file.seekg(0, std::ios::end);
            std::streamsize size = file.tellg();
            file.seekg(0, std::ios::beg);

            std::vector<std::byte> bytes(size);
            file.read(reinterpret_cast<char *>(bytes.data()), size);
            return bytes;
        }

        return {};
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

    const auto font = service.load_font(std::move(bytes)).get();

    EXPECT_FALSE(font->family_name().empty());
    EXPECT_FALSE(font->style_name().empty());
}
