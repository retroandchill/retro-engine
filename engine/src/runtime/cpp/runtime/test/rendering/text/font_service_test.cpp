/**
 * @file font_service_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import retro.runtime.rendering.text.font;
import std;
import retro.runtime.testing.files;
import retro.runtime.rendering.headless_render_backend;

using namespace retro;

TEST(FontService, LoadFontFace)
{
    HeadlessRenderBackend render_backend{};
    const FontService service{render_backend};

    auto bytes = retro::files::test_font | std::ranges::to<std::vector>();
    ASSERT_FALSE(bytes.empty());

    const auto font = service.load_font(std::move(bytes)).get();

    EXPECT_FALSE(font->family_name().empty());
    EXPECT_FALSE(font->style_name().empty());
}
