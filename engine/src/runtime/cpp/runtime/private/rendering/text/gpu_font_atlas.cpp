/**
 * @file gpu_font_atlas.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.text.gpu_font_atlas;

namespace retro
{

    RefCountPtr<GpuFontAtlas> FontAtlasCache::get_or_create(const FontFace &font, const FontSdfConfig &config)
    {
        FontAtlasKey key{
            .face = font.id(),
            .pixel_size = config.pixel_size,
            .padding = config.padding,
            .spread = config.spread,
            .first_codepoint = config.first_codepoint,
            .last_codepoint = config.last_codepoint,
        };

        if (const auto existing = atlases_.find(key); existing != atlases_.end())
        {
            return existing->second;
        }

        auto [width, height, metrics, pixels, glyphs] = font.create_sdf_atlas(config);
        auto atlas = make_ref_counted<GpuFontAtlas>(
            GpuFontAtlas::ConstructTag{},
            metrics,
            std::move(glyphs),
            render_backend_.upload_texture(pixels, width, height, TextureFormat::r8, TextureFilter::linear));
        atlases_.emplace(key, atlas);
        return atlas;
    }
} // namespace retro
