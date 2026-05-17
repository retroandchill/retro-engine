/**
 * @file gpu_font_atlas.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.runtime.rendering.text.gpu_font_atlas;

import std;
import retro.runtime.rendering.text.font;
import retro.core.containers.optional;
import retro.runtime.rendering.texture;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.rendering.text.font_service;
import retro.core.algorithm.hashing;
import retro.runtime.rendering.render_backend;

namespace retro
{
    export class FontAtlasCache;

    struct FontAtlasKey
    {
        const std::uint64_t face{};
        std::uint32_t pixel_size{};
        std::uint32_t padding{};
        float spread{};
        char32_t first_codepoint{};
        char32_t last_codepoint{};

        constexpr friend bool operator==(const FontAtlasKey &lhs, const FontAtlasKey &rhs) noexcept = default;
    };
} // namespace retro

template <>
struct std::hash<retro::FontAtlasKey>
{
    std::size_t operator()(const retro::FontAtlasKey &key) const noexcept
    {
        return retro::hash_combine(key.face,
                                   key.pixel_size,
                                   key.padding,
                                   key.spread,
                                   key.first_codepoint,
                                   key.last_codepoint);
    }
};

namespace retro
{
    export class GpuFontAtlas final : public IntrusiveRefCounted
    {
        struct ConstructTag
        {
        };

      public:
        GpuFontAtlas(ConstructTag,
                     const FontMetrics &metrics,
                     std::unordered_map<char32_t, GlyphMetrics> glyphs,
                     RefCountPtr<Texture> texture)
            : metrics_{metrics}, glyphs_{std::move(glyphs)}, texture_{std::move(texture)}
        {
        }

        [[nodiscard]] inline const FontMetrics &metrics() const noexcept
        {
            return metrics_;
        }

        [[nodiscard]] inline Optional<const GlyphMetrics &> find_glyph(const char32_t codepoint) const noexcept
        {
            if (const auto it = glyphs_.find(codepoint); it != glyphs_.end())
            {
                return it->second;
            }
            return std::nullopt;
        }

        [[nodiscard]] inline const Texture &texture() const noexcept
        {
            return *texture_;
        }

      private:
        friend class FontAtlasCache;

        FontMetrics metrics_;
        std::unordered_map<char32_t, GlyphMetrics> glyphs_;
        RefCountPtr<Texture> texture_;
    };

    class RETRO_API FontAtlasCache
    {
      public:
        explicit inline FontAtlasCache(RenderBackend &render_backend) : render_backend_{render_backend}
        {
        }

        RefCountPtr<GpuFontAtlas> get_or_create(const FontFace &font, const FontSdfConfig &config);

      private:
        RenderBackend &render_backend_;
        std::unordered_map<FontAtlasKey, RefCountPtr<GpuFontAtlas>> atlases_;
    };
} // namespace retro
