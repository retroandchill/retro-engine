/**
 * @file font.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.text.font;

import std;
import retro.core.math.vector;
import retro.core.util.color;

namespace retro
{
    export struct FontSdfConfig
    {
        std::uint32_t pixel_size{48};

        std::uint32_t atlas_width{1024};
        std::uint32_t atlas_height{1024};

        std::uint32_t padding{8};
        float spread{8.0f};

        char32_t first_codepoint{32};
        char32_t last_codepoint{126};
    };

    export struct FontMetrics
    {
        float ascender{};
        float descender{};
        float line_height{};
        float underline_position{};
        float underline_thickness{};
    };

    export struct GlyphMetrics
    {
        char32_t codepoint{};
        std::uint32_t glyph_index{};

        float advance_x{};
        float bearing_x{};
        float bearing_y{};

        float width{};
        float height{};

        float uv_min_x{};
        float uv_min_y{};
        float uv_max_x{};
        float uv_max_y{};
    };

    export struct FontAtlas
    {
        std::uint32_t width{};
        std::uint32_t height{};

        FontMetrics metrics{};

        std::vector<std::byte> pixels{};
        std::unordered_map<char32_t, GlyphMetrics> glyphs{};
    };

    export struct RasterizedGlyph
    {
        char32_t codepoint{};
        std::uint32_t glyph_index{};

        float advance_x{};
        float bearing_x{};
        float bearing_y{};

        float width{};
        float height{};

        std::vector<std::byte> coverage{};
    };

    struct SdfGlyphBitmap
    {
        char32_t codepoint{};
        std::uint32_t glyph_index{};

        float advance_x{};
        float bearing_x{};
        float bearing_y{};

        std::uint32_t width{};
        std::uint32_t height{};

        std::uint32_t padding{};

        std::vector<std::byte> pixels;
    };

    struct AtlasPacker
    {
        std::uint32_t width{};
        std::uint32_t height{};

        std::uint32_t cursor_x{};
        std::uint32_t cursor_y{};
        std::uint32_t row_height{};

        [[nodiscard]] std::optional<std::pair<std::uint32_t, std::uint32_t>> allocate(std::uint32_t glyph_width,
                                                                                      std::uint32_t glyph_height);
    };

    export struct TextVertex
    {
        Vector2f position{};
        Vector2f texcoord{};
        Color color{};
    };

    export struct TextLayoutGlyph
    {
        char32_t codepoint{};
        std::uint32_t glyph_index{};

        float x{};
        float y{};

        GlyphMetrics metrics{};
    };

    export struct TextLayout
    {
        std::vector<TextLayoutGlyph> glyphs;

        float width{};
        float height{};
    };

    export enum class TextHorizontalAlign
    {
        left,
        center,
        right,
    };

    export enum class TextVerticalAlign
    {
        top,
        middle,
        bottom,
        baseline,
    };

    export struct TextLayoutConfig
    {
        float max_width{std::numeric_limits<float>::infinity()};
        float line_spacing{1.0f};

        TextHorizontalAlign horizontal_align{TextHorizontalAlign::left};
        TextVerticalAlign vertical_align{TextVerticalAlign::top};
    };
} // namespace retro
