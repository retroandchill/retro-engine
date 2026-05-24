/**
 * @file buffer_writer.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.io.buffer_writer;

import std;

namespace retro
{
    export template <typename Writer>
    concept BufferWriter = requires(Writer &writer, std::size_t size) {
        typename Writer::value_type;
        {
            writer.advance(size)
        };
        {
            writer.get_span(size)
        } -> std::same_as<std::span<typename Writer::value_type>>;
    };

    export template <BufferWriter Writer, std::ranges::input_range Range>
        requires std::ranges::sized_range<Range> &&
                 std::convertible_to<std::ranges::range_reference_t<Range>, typename Writer::value_type>
    constexpr void write(Writer &writer, Range &&data)
    {
        auto size = std::ranges::size(data);
        auto dest = writer.get_span(size);
        std::size_t i = 0;
        for (auto &&elem : std::forward<Range>(data))
        {
            dest[i++] = std::forward<decltype(elem)>(elem);
        }
        writer.advance(size);
    }
} // namespace retro
