/**
 * @file font.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.text.font;

namespace retro
{
    std::optional<std::pair<std::uint32_t, std::uint32_t>> AtlasPacker::allocate(std::uint32_t glyph_width,
                                                                                 std::uint32_t glyph_height)
    {
        if (glyph_width == 0 || glyph_height == 0)
        {
            return std::pair{cursor_x, cursor_y};
        }

        if (glyph_width > width || glyph_height > height)
        {
            return std::nullopt;
        }

        if (cursor_x + glyph_width > width)
        {
            cursor_x = 0;
            cursor_y += row_height;
            row_height = 0;
        }

        if (cursor_y + glyph_height > height)
        {
            return std::nullopt;
        }

        auto position = std::make_pair(cursor_x, cursor_y);

        cursor_x += glyph_width;
        row_height = std::max(row_height, glyph_height);

        return position;
    }
} // namespace retro
