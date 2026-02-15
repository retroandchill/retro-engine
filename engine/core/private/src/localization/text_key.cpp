/**
 * @file text_key.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>
#include <cstddef>

module retro.core.localization;

import :text_key;
import retro.core.type_traits.range;
import retro.core.util.guid;
import retro.core.memory.arena_allocator;

namespace retro
{
    constexpr std::size_t text_key_elements_min_hash_size = 32768;
    constexpr std::size_t text_key_max_size_bytes = 1000;

    using TextKeyCharType = char;
    using TextKeyStringView = std::basic_string_view<TextKeyCharType>;

    template <typename ElementType, std::uint32_t ChunkCursorBits = 16>
        requires(std::is_trivially_destructible_v<ElementType>)
    class ChunkedPackedArray
    {
      public:
        static constexpr std::uint32_t bytes_per_chunk = 1 << ChunkCursorBits;

        std::int32_t add(const std::uint32_t element_size)
        {
            if (chunks_.empty() || chunk_cursors_.back() + element_size > bytes_per_chunk)
            {
                allocate_new_chunk();
            }

            const std::size_t current_chunk = chunks_.size() - 1;
            const std::size_t result_index = (current_chunk << ChunkCursorBits) + chunk_cursors_[current_chunk];
            chunk_cursors_[current_chunk] += element_size;
            count_++;

            return result_index;
        }

        [[nodiscard]] std::int32_t first_index() const
        {
            return size() > 0 ? 0 : index_none<std::int32_t>;
        }

        [[nodiscard]] std::int32_t get_next_index(const std::int32_t element_index,
                                                  const std::uint32_t element_size) const
        {
            std::uint32_t chunk_index = element_index >> ChunkCursorBits;
            std::uint32_t chunk_cursor = element_index & ((1 << ChunkCursorBits) - 1);

            chunk_cursor += element_size;

            if (chunk_cursor >= chunk_cursors_[chunk_index])
            {
                ++chunk_index;
                chunk_cursor = 0;
            }

            return (chunk_index << ChunkCursorBits) | chunk_cursor;
        }

        ElementType &get(const std::int32_t element_index) const
        {
            const std::uint32_t chunk_index = element_index >> ChunkCursorBits;
            const std::uint32_t chunk_cursor = element_index & ((1 << ChunkCursorBits) - 1);

            return *reinterpret_cast<ElementType *>(chunks_[chunk_index].get() + chunk_cursor);
        }

        [[nodiscard]] std::uint32_t size() const noexcept
        {
            return count_;
        }

      private:
        std::vector<std::unique_ptr<std::byte[]>> chunks_;
        std::vector<std::uint32_t> chunk_cursors_;
        std::uint32_t count_ = 0;

        void allocate_new_chunk()
        {
            chunks_.emplace_back(std::make_unique<std::byte[]>(bytes_per_chunk));
            chunk_cursors_.emplace_back(0);
        }
    };

    class TextKeyState
    {
      public:
        TextKey find_or_add(std::u16string_view str)
        {
            assert(!str.empty());

            const std::uint32_t string_hash = text::hash_string(str);

            return find_or_add_internal(str, string_hash);
        }

        TextKey find_or_add(std::u16string_view str, const std::size_t string_hash)
        {
            assert(!str.empty());
            return find_or_add_internal(str, string_hash);
        }

        void append_string_by_index(const std::int32_t index, std::u16string &output)
        {
            assert(index != index_none<std::int32_t>);

            std::shared_lock lock{mutex_};

            auto &key_data = key_data_allocations_.get(index);
            if (key_data.is_string_view())
            {
                output.append_range(key_data.as_string_view());
            }
            else
            {
                key_data.as_guid().append_string(output);
            }
        }

        std::size_t get_hash_by_index(const std::int32_t index) const noexcept
        {
            assert(index != index_none<std::int32_t>);

            std::shared_lock lock{mutex_};
            return key_data_allocations_.get(index).string_hash;
        }

        static TextKeyState &state()
        {
            static TextKeyState state;
            return state;
        }

      private:
        struct KeyDataView
        {
            KeyDataView(TextKeyStringView view, std::size_t string_hash)
                : string_length(view.size()), string_hash(string_hash), data_ptr(view.data())
            {
            }

            KeyDataView(const Guid &guid, const std::size_t string_hash)
                : string_length(index_none<std::size_t>), string_hash(string_hash), data_ptr(guid.data())
            {
            }

            bool is_string_view() const noexcept
            {
                return string_length != index_none<std::size_t>;
            }

            bool is_guid() const noexcept
            {
                return !is_string_view();
            }

            const TextKeyStringView &as_string_view() const noexcept
            {
                assert(is_string_view());
                return TextKeyStringView{static_cast<const char *>(data_ptr), string_length};
            }

            const Guid &as_guid() const noexcept
            {
                assert(is_guid());
                return *static_cast<const Guid *>(data_ptr);
            }

            std::size_t string_length{};
            std::size_t string_hash{};
            const void *data_ptr;
        };

        struct KeyData
        {
            bool is_string_view() const noexcept
            {
                return string_length != index_none<std::size_t>;
            }

            TextKeyStringView as_string_view() const noexcept
            {
                assert(is_string_view());
                return TextKeyStringView{static_cast<const char *>(data_ptr), string_length};
            }

            bool is_guid() const noexcept
            {
                return !is_string_view();
            }

            const Guid &as_guid() const noexcept
            {
                assert(is_guid());
                return *static_cast<const Guid *>(data_ptr);
            }

            std::int32_t next_element_index = index_none<std::int32_t>;
            std::size_t string_hash;

            std::size_t string_length;
            const void *data_ptr;
        };

        static constexpr std::uint32_t key_data_header_size_bytes = offsetof(KeyData, data_ptr);

        template <typename KeyDataA, typename KeyDataB>
        friend bool operator==(const KeyDataA &a, const KeyDataB &b)
        {
            if (a.string_length != b.string_length)
                return false;

            if (a.is_string_view())
            {
                return std::memcmp(a.data_ptr, b.data_ptr, a.string_length * sizeof(TextKeyCharType)) == 0;
            }

            return a.as_guid() == b.as_guid();
        }

        TextKey find_or_add_internal(std::u16string_view str, const std::size_t string_hash)
        {
            assert(false);
        }

        class KeyDataAllocator
        {
          public:
            KeyDataAllocator() = default;

            std::int32_t add(TextKeyStringView view, std::uint32_t string_hash) noexcept
            {
                assert(view.size() * sizeof(TextKeyCharType) < std::numeric_limits<std::uint32_t>::max());
                const auto data_size_bytes = static_cast<std::uint32_t>(view.size() * sizeof(TextKeyCharType));

                const std::int32_t new_element_index = add(data_size_bytes, string_hash);
                auto &key_data = elements_.get(new_element_index);
                key_data.string_length = view.size();
                void *data_buffer = get_persistent_arena().allocate(data_size_bytes, alignof(TextKeyCharType));
                std::memcpy(data_buffer, view.data(), data_size_bytes);
                key_data.data_ptr = data_buffer;

                return new_element_index;
            }

            std::int32_t add(const Guid &key_guid, std::uint32_t string_hash) noexcept
            {
                constexpr std::uint32_t data_size_bytes = sizeof(Guid);
                const std::int32_t new_element_index = add(data_size_bytes, string_hash);

                auto &key_data = elements_.get(new_element_index);
                key_data.string_length = index_none<std::size_t>;
                auto *guid_ptr = static_cast<Guid *>(get_persistent_arena().allocate(data_size_bytes, alignof(Guid)));
                key_data.data_ptr = guid_ptr;
                *guid_ptr = key_guid;

                return new_element_index;
            }

            std::int32_t find(const KeyDataView &key_data)
            {
                if (hash_size_ == 0)
                    return index_none<std::int32_t>;

                const std::uint32_t key_data_hash = key_data.string_hash;
                const std::uint32_t hash_index = key_data_hash & (hash_size_ - 1);

                std::int32_t element_index = hash_[hash_index];
                while (element_index != index_none<std::int32_t>)
                {
                    const auto &element = elements_.get(element_index);
                    if (element == key_data)
                        return element_index;

                    element_index = element.next_element_index;
                }

                return index_none<std::int32_t>;
            }

            const KeyData &get(std::int32_t index) const noexcept
            {
                return elements_.get(index);
            }

            std::uint32_t size() const noexcept
            {
                return elements_.size();
            }

          private:
            void allocate_hash()
            {
                hash_ = std::make_unique<std::int32_t[]>(hash_size_);
                for (std::uint32_t i = 0; i < hash_size_; ++i)
                {
                    hash_[i] = index_none<std::int32_t>;
                }
            }

            void conditional_rehash(const std::int32_t num_elements)
            {
                constexpr std::uint32_t default_number_elements_per_bucket = 2;
                const std::uint32_t new_hash_size =
                    std::max(min_hash_size, std::bit_ceil(num_elements / default_number_elements_per_bucket));
                if (new_hash_size <= hash_size_)
                    return;

                hash_size_ = new_hash_size;
                allocate_hash();

                std::int32_t element_index = elements_.first_index();
                for (auto &element : elements_)
                {
                    const std::uint32_t element_hash = element.string_hash;
                    const std::uint32_t hash_index = element_hash & (hash_size_ - 1);

                    element.next_element_index = hash_[hash_index];
                    hash_[hash_index] = element_index;

                    element_index = elements_.get_next_index(element_index, sizeof(KeyData));
                }
            }

            std::int32_t add(const std::uint32_t element_size, const std::uint32_t string_hash) noexcept
            {
                conditional_rehash(elements_.size() + 1);

                const std::uint32_t hash_index = string_hash & (hash_size_ - 1);
                const std::int32_t new_element_index = elements_.add(sizeof(KeyData));

                auto &new_element = elements_.get(new_element_index);
                new_element.string_hash = string_hash;
                new_element.next_element_index = hash_[hash_index];
                hash_[hash_index] = new_element_index;

                return new_element_index;
            }

            static constexpr std::uint32_t min_hash_size = text_key_elements_min_hash_size;

            ChunkedPackedArray<KeyData> elements_;
            std::uint32_t hash_size_ = 0;
            std::unique_ptr<std::int32_t[]> hash_ = nullptr;
        };

        mutable std::shared_mutex mutex_;
        KeyDataAllocator key_data_allocations_;
    };
} // namespace retro
