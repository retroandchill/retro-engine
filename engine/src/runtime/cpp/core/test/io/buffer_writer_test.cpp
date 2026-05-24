/**
 * @file buffer_writer_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import std;
import retro.core.io.buffer_writer;
import retro.core.io.array_buffer_writer;

using namespace retro;

TEST(BufferWriter, WritingToByteBuffer)
{
    ArrayBufferWriter<std::byte> writer{1024};

    static_assert(BufferWriter<ArrayBufferWriter<std::byte>>);

    std::array data = {std::byte{1}, std::byte{2}, std::byte{3}};
    write(writer, data);
    EXPECT_EQ(writer.written_count(), 3);
    EXPECT_EQ(writer.capacity(), 1024);
    EXPECT_EQ(writer.free_capacity(), 1021);

    const auto span = writer.written_span();
    ASSERT_EQ(span.size(), 3);
    EXPECT_EQ(span[0], std::byte{1});
    EXPECT_EQ(span[1], std::byte{2});
    EXPECT_EQ(span[2], std::byte{3});
}
