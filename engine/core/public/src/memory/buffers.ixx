/**
 * @file buffers.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

export module retro.core.memory.buffers;

import std;

namespace retro
{
    /**
     * Copies the value of a trivially copyable and destructible type into the corresponding byte buffer.
     *
     * @tparam T The type we're copying
     * @param buffer The buffer we're writing the bytes into
     * @param value The value we're copying in
     * @return A pointer to the value that we wrote into the buffer
     */
    export template <typename T>
        requires std::is_trivially_copyable_v<T> && std::is_trivially_destructible_v<T>
    T *write_to_buffer(const std::span<std::byte> buffer, const T &value)
    {
        assert(buffer.size() >= sizeof(T) && "Buffer too small for type T");

        auto *data = reinterpret_cast<T *>(buffer.data());
        *data = value;
        return data;
    }

    /**
     * Constructs a trivially copyable and destructible type into the specified byte buffer.
     *
     * @tparam T The type we're going to treat the buffer as
     * @tparam Args The types of the constructor arguments
     * @param buffer The buffer we're writing the bytes into
     * @param args The forwarded constructor arguments
     * @return A pointer to the value that we wrote into the buffer
     */
    export template <typename T, typename... Args>
        requires std::is_trivially_copyable_v<T> && std::is_trivially_destructible_v<T> &&
                 std::constructible_from<T, Args...>
    T *emplace_to_buffer(const std::span<std::byte> buffer, Args &&...args)
    {
        assert(buffer.size() >= sizeof(T) && "Buffer too small for type T");

        auto *data = reinterpret_cast<T *>(buffer.data());
        std::construct_at<T>(data, std::forward<Args>(args)...);
        return data;
    }

} // namespace retro
