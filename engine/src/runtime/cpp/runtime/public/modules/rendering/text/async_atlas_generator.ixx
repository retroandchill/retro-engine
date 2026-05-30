/**
 * @file font_atlas_generator.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.text.async_atlas_generator;

import std;
import msdfgen;
import msdf_atlas;
import retro.core.async.task;
import retro.core.async.workload;

namespace retro
{
    template <typename AtlasStorage, typename T, std::int32_t N>
    concept ValidAtlasStorage =
        std::movable<AtlasStorage> &&
        std::constructible_from<AtlasStorage, AtlasStorage &&, std::int32_t, std::int32_t> &&
        std::constructible_from<AtlasStorage,
                                AtlasStorage &&,
                                std::int32_t,
                                std::int32_t,
                                const msdf_atlas::Remap *,
                                std::int32_t> &&
        requires(AtlasStorage &storage, std::int32_t l, std::int32_t b, msdfgen::BitmapConstSection<T, N> bitmap) {
            {
                storage.put(l, b, bitmap)
            };
        };

    export template <typename T,
                     std::int32_t N,
                     msdf_atlas::GeneratorFunction<T, N> Generator,
                     ValidAtlasStorage<T, N> AtlasStorage>
    class AsyncAtlasGenerator
    {
      public:
        AsyncAtlasGenerator() = default;

        template <typename... Args>
            requires std::constructible_from<AtlasStorage, std::int32_t, std::int32_t, Args...>
        explicit AsyncAtlasGenerator(std::int32_t width, std::int32_t height, Args &&...args)
            : storage_{width, height, std::forward<Args>(args)...}
        {
        }

        void generate(const msdf_atlas::GlyphGeometry *glyphs, const std::int32_t glyph_count)
        {
            generate(std::span{glyphs, static_cast<std::size_t>(glyph_count)});
        }

        void generate(const std::span<const msdf_atlas::GlyphGeometry> glyphs)
        {
            generate_async(glyphs).wait();
        }

        Task<> generate_async(const msdf_atlas::GlyphGeometry *glyphs,
                              const std::int32_t glyph_count,
                              std::stop_token stop_token = {})
        {
            return generate_async(std::span{glyphs, static_cast<std::size_t>(glyph_count)}, std::move(stop_token));
        }

        Task<> generate_async(const std::span<const msdf_atlas::GlyphGeometry> glyphs, std::stop_token stop_token = {})
        {
            std::int32_t max_box_area = 0;
            for (const msdf_atlas::GlyphBox box : glyphs)
            {
                max_box_area = std::max(max_box_area, box.rect.w * box.rect.h);
            }
            const std::int32_t thread_buffer_size = N * max_box_area;
            if (thread_count_ * thread_buffer_size > static_cast<std::int32_t>(glyph_buffer_.size()))
                glyph_buffer_.resize(thread_count_ * thread_buffer_size);
            if (thread_count_ * max_box_area > static_cast<std::int32_t>(error_correction_buffer_.size()))
                error_correction_buffer_.resize(thread_count_ * max_box_area);
            auto thread_attributes = std::views::iota(0, thread_count_) |
                                     std::views::transform(
                                         [this, max_box_area](const std::int32_t i)
                                         {
                                             auto attributes = attributes_;
                                             attributes.config.errorCorrection.buffer =
                                                 std::next(error_correction_buffer_.data(), i * max_box_area);
                                             return attributes;
                                         }) |
                                     std::ranges::to<std::vector>();

            co_await Workload{
                [this, &glyphs, &thread_attributes, thread_buffer_size](std::size_t i, std::size_t thread_no)
                {
                    if (auto &glyph = glyphs[i]; !glyph.isWhitespace())
                    {
                        std::int32_t l;
                        std::int32_t b;
                        std::int32_t w;
                        std::int32_t h;
                        glyph.getBoxRect(l, b, w, h);
                        msdfgen::BitmapRef<T, N> glyph_bitmap{
                            std::next(glyph_buffer_.data(), thread_no * thread_buffer_size),
                            w,
                            h};
                        Generator(glyph_bitmap, glyph, thread_attributes[thread_no]);
                        storage_.put(l, b, msdfgen::BitmapConstSection<T, N>(glyph_bitmap));
                    }
                    return true;
                },
                glyphs.size()}
                .finish_async(thread_count_, stop_token);
        }

        void rearrange(std::int32_t width, std::int32_t height, const msdf_atlas::Remap *remapping, std::int32_t count)
        {
            rearrange(width, height, std::span{remapping, static_cast<std::size_t>(count)});
        }

        void rearrange(std::int32_t width, std::int32_t height, const std::span<const msdf_atlas::Remap> remapping)
        {
            for (auto &mapping : remapping)
            {
                layout_[mapping.index].rect.x = mapping.target.x;
                layout_[mapping.index].rect.y = mapping.target.y;
            }
            AtlasStorage new_storage{std::move(storage_),
                                     width,
                                     height,
                                     remapping.data(),
                                     static_cast<std::int32_t>(remapping.size())};
            storage_ = std::move(new_storage);
        }

        void resize(std::int32_t width, std::int32_t height)
        {
            AtlasStorage new_storage{std::move(storage_), width, height};
            storage_ = std::move(new_storage);
        }

        void set_attributes(const msdf_atlas::GeneratorAttributes &attributes)
        {
            attributes_ = attributes;
        }

        void set_thread_count(const std::int32_t thread_count)
        {
            thread_count_ = thread_count;
        }

        [[nodiscard]] const AtlasStorage &atlas_storage() const
        {
            return storage_;
        }

        [[nodiscard]] const std::vector<msdf_atlas::GlyphBox> &layout() const
        {
            return layout_;
        }

      private:
        AtlasStorage storage_;
        std::vector<msdf_atlas::GlyphBox> layout_;
        std::vector<T> glyph_buffer_;
        std::vector<msdfgen::byte> error_correction_buffer_;
        msdf_atlas::GeneratorAttributes attributes_;
        std::int32_t thread_count_{1};
    };
} // namespace retro
