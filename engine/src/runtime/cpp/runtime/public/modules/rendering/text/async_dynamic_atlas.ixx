/**
 * @file async_dynamic_atlas.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.text.font:async_dynamic_atlas;

import retro.core.util.enum_class_flags;

import :dependencies;
import std;
import retro.core.async.task;

namespace retro
{

    enum class AtlasChangeFlag
    {
        no_change = 0,
        resized = 1 << 0,
        rearranged = 1 << 1
    };

    export template <>
    constexpr bool is_flag_enum<AtlasChangeFlag> = true;

    template <typename AtlasGenerator>
    concept ValidAtlasGenerator =
        std::is_default_constructible_v<AtlasGenerator> && std::movable<AtlasGenerator> &&
        requires(AtlasGenerator &generator, std::int32_t side, std::span<const msdf_atlas::Remap> remap_buffer) {
            {
                generator.rearrange(side, side, remap_buffer)
            };
            {
                generator.resize(side, side)
            };
        } && requires(AtlasGenerator &generator, std::span<msdf_atlas::GlyphGeometry> geometries) {
            {
                generator.generate(geometries)
            };
            {
                generator.generate_async(geometries)
            } -> Awaitable;
        };

    template <ValidAtlasGenerator AtlasGenerator>
    class AsyncDynamicAtlas
    {
      public:
        AsyncDynamicAtlas() = default;

        template <typename... Args>
        explicit AsyncDynamicAtlas(const std::int32_t min_side, Args &&...args)
            : side_{min_side > 0 ? msdf_atlas::ceilToPOT(min_side) : 0}, packer_{side_ + spacing_, side_ + spacing_},
              generator_{side_, side_, std::forward<Args>(args)...}
        {
        }

        explicit AsyncDynamicAtlas(AtlasGenerator &&generator) : generator_{std::move(generator)}
        {
        }

        AtlasChangeFlag add(std::span<msdf_atlas::GlyphGeometry> glyphs, bool allow_rearrange = false)
        {
            const auto change_flag = adjust_atlas_if_needed(glyphs, allow_rearrange);
            generator_.generate(glyphs);
            return change_flag;
        }

        Task<AtlasChangeFlag> add_async(std::span<msdf_atlas::GlyphGeometry> glyphs, const bool allow_rearrange = false)
        {
            auto change_flag = adjust_atlas_if_needed(glyphs, allow_rearrange);
            co_await generator_.generate_async(glyphs);
            co_return change_flag;
        }

        AtlasGenerator &atlas_generator()
        {
            return generator_;
        }

        const AtlasGenerator &atlas_generator() const
        {
            return generator_;
        }

      private:
        AtlasChangeFlag adjust_atlas_if_needed(std::span<msdf_atlas::GlyphGeometry> glyphs, bool allow_rearrange)
        {
            auto change_flag = AtlasChangeFlag::no_change;
            const auto start = rectangles_.size();
            for (const auto [i, glyph] : glyphs | std::views::enumerate)
            {
                if (glyph.isWhitespace())
                    continue;

                std::int32_t w;
                std::int32_t h;
                glyph.getBoxSize(w, h);
                msdf_atlas::Rectangle rect{0, 0, w + spacing_, h + spacing_};
                rectangles_.push_back(rect);
                remap_buffer_.push_back(msdf_atlas::Remap{
                    .index = static_cast<std::int32_t>(glyph_count_ + i),
                    .width = w,
                    .height = h,
                });
                total_area_ += (w + spacing_) * (h + spacing_);
            }

            if (rectangles_.size() > start)
            {
                auto packer_start = start;
                std::int32_t remaining;
                while ((remaining = packer_.pack(std::next(rectangles_.data(), packer_start),
                                                 static_cast<std::int32_t>(rectangles_.size() - packer_start))) > 0)
                {
                    side_ = side_ == 0 ? 2 : side_ * 2;
                    while (side_ * side_ < total_area_)
                        side_ *= 2;

                    if (allow_rearrange)
                    {
                        packer_ = msdf_atlas::RectanglePacker{side_ + spacing_, side_ + spacing_};
                        packer_start = 0;
                    }
                    else
                    {
                        packer_.expand(side_ + spacing_, side_ + spacing_);
                        packer_start = rectangles_.size() - remaining;
                    }
                    change_flag |= AtlasChangeFlag::resized;
                }

                if (packer_start < start)
                {
                    for (std::size_t i = packer_start; i < start; ++i)
                    {
                        auto &remap = remap_buffer_[i];
                        remap.source = remap.target;
                        remap.target.x = rectangles_[i].x;
                        remap.target.y = rectangles_[i].y;
                    }
                    generator_.rearrange(side_, side_, std::span{remap_buffer_}.subspan(0, start));
                }
                else if (has_any_flags(change_flag, AtlasChangeFlag::resized))
                {
                    generator_.resize(side_, side_);
                }

                for (std::size_t i = start; i < rectangles_.size(); ++i)
                {
                    auto &remap = remap_buffer_[i];
                    const auto &rect = rectangles_[i];
                    remap.target.x = rect.x;
                    remap.target.y = rect.y;
                    glyphs[remap.index - glyph_count_].placeBox(rect.x, rect.y);
                }
            }

            return change_flag;
        }

        std::int32_t side_ = 0;
        std::int32_t spacing_ = 0;
        std::int32_t glyph_count_ = 0;
        std::int32_t total_area_ = 0;
        msdf_atlas::RectanglePacker packer_{};
        AtlasGenerator generator_{};
        std::vector<msdf_atlas::Rectangle> rectangles_{};
        std::vector<msdf_atlas::Remap> remap_buffer_{};
    };
} // namespace retro
