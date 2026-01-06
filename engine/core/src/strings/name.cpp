/**
 * @file name.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/dll/detail/aggressive_ptr_cast.hpp>
#include <cassert>
#include <fmt/base.h>
#include <uni_algo/case.h>

#define RETRO_WITH_CASE_PRESERVING_NAME _DEBUG

module retro.core;

import cityhash;
import uni_algo;

namespace retro
{
    struct NameEntryIds
    {
        NameEntryId comparison_id;
        NameEntryId display_id;
    };

    template <Char From, Char To>
    To *convert_in_place(From *str, uint32 length)
    {
        if constexpr (std::same_as<From, To>)
        {
            return str;
        }
        else
        {
            for (uint32 i = length; i > 0; --i)
            {
                std::bit_cast<To *>(str)[i] = str[i];
            }

            return std::bit_cast<To *>(str);
        }
    }

    union NameBuffer
    {
        std::array<char, NAME_SIZE> utf8_name_;
        std::array<char16_t, NAME_SIZE> utf16_name_;
    };

    struct NameStringView
    {

        union
        {
            const void *data{nullptr};
            const char *utf8;
            const char16_t *utf16;
        };

        uint32 length{0};
        bool is_utf16{false};

        NameStringView() = default;

        explicit NameStringView(const std::string_view str) : utf8(str.data()), length(str.length())
        {
        }

        explicit NameStringView(const std::u16string_view str) : utf16(str.data()), length(str.length()), is_utf16(true)
        {
        }

        [[nodiscard]] bool is_utf8() const
        {
            return !is_utf16;
        }
        [[nodiscard]] bool is_none_string() const;

        [[nodiscard]] usize bytes_with_terminator() const
        {
            return (length + 1) * (is_utf16 ? sizeof(char16_t) : sizeof(char));
        }

        [[nodiscard]] usize bytes_without_terminator() const
        {
            return length * (is_utf16 ? sizeof(char16_t) : sizeof(char));
        }
    };

    template <NameCase Sensitivity>
    bool equals_same_dimensions(const NameStringView a, const NameStringView b)
    {
        assert(a.length == b.length && a.is_utf8() == b.is_utf8());

        if constexpr (Sensitivity == NameCase::CaseSensitive)
        {
            return b.is_utf8() ? una::casesens::compare_utf8({a.utf8, a.length}, {b.utf8, b.length}) == 0
                               : una::casesens::compare_utf16({a.utf16, a.length}, {b.utf16, b.length}) == 0;
        }
        else
        {
            return b.is_utf8() ? una::caseless::compare_utf8({a.utf8, a.length}, {b.utf8, b.length}) == 0
                               : una::caseless::compare_utf16({a.utf16, a.length}, {b.utf16, b.length}) == 0;
        }
    }

    template <NameCase Sensitivity>
    bool equals(const NameStringView a, const NameStringView b)
    {
        return a.length == b.length && a.is_utf8() == b.is_utf8() && equals_same_dimensions<Sensitivity>(a, b);
    }

    template <NameCase Sensitivity>
    bool equals_same_dimensions(const NameEntry &entry, const NameStringView name)
    {
        NameBuffer decode_buffer;
        return equals_same_dimensions<Sensitivity>(entry.make_view(decode_buffer), name);
    }

    static constexpr uint32 NAME_MAX_BLOCK_BITS = 13;
    static constexpr uint32 NAME_BLOCK_OFFSET_BITS = 16;
    static constexpr uint32 NAME_MAX_BLOCKS = 1 << NAME_MAX_BLOCK_BITS;
    static constexpr uint32 NAME_BLOCK_OFFSETS = 1 << NAME_BLOCK_OFFSET_BITS;
    static constexpr uint32 NAME_ENTRY_ID_BITS = NAME_BLOCK_OFFSET_BITS + NAME_MAX_BLOCK_BITS;
    static constexpr uint32 NAME_ENTRY_ID_MASK = (1 << NAME_ENTRY_ID_BITS) - 1;

    struct DefaultNameStore
    {
        static uint32 try_place(std::byte *out_entry, uint32 out_size, NameStringView name)
        {
            uint32 bytes = get_default_size(name.length, name.is_utf16);
            return bytes <= out_size ? bytes : 0;
        }

        static void finalize(std::byte *out_name_data, NameStringView name)
        {
            std::memcpy(out_name_data, name.data, name.bytes_without_terminator());
        }

        static void copy(char *out, const std::byte *name_data, uint32 name_length)
        {
            std::memcpy(out, name_data, sizeof(char) * name_length);
        }

        static void copy(char16_t *out, const std::byte *name_data, uint32 name_length)
        {
            std::memcpy(out, name_data, name_length);
        }

        static uint32 get_size(const NameEntry &entry)
        {
            return get_default_size(static_cast<uint32>(entry.name_length()), entry.is_utf16());
        }

        static const char *try_get_utf8(const void *name_data)
        {
            return static_cast<const char *>(name_data);
        }

        static const char16_t *try_get_utf16(const void *name_data)
        {
            return static_cast<const char16_t *>(name_data);
        }

      private:
        static uint32 get_default_size(uint32 length, bool is_utf16)
        {
            constexpr uint32 header_size = NameEntry::data_offset();
            const uint32 footer_size = length * (is_utf16 ? sizeof(char16_t) : sizeof(char));
            return header_size + footer_size;
        }
    };

    using NameStore = DefaultNameStore;

    struct NameEntryHandle
    {
        uint32 block = 0;
        uint32 offset = 0;

        NameEntryHandle(const uint32 block, const uint32 offset) : block(block), offset(offset)
        {
            assert(block < NAME_MAX_BLOCKS);
            assert(offset < NAME_BLOCK_OFFSETS);
        }

        explicit(false) NameEntryHandle(NameEntryId id)
            : block{id.to_unstable_int() >> NAME_BLOCK_OFFSET_BITS},
              offset{id.to_unstable_int() & NAME_BLOCK_OFFSETS - 1}
        {
        }

        explicit(false) operator NameEntryId() const
        {
            return NameEntryId::from_unstable_int(block << NAME_BLOCK_OFFSET_BITS | offset);
        }

        explicit operator bool() const
        {
            return block | offset;
        }
    };
} // namespace retro

template <>
struct std::hash<retro::NameEntryHandle>
{
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::NameEntryHandle &handle) const noexcept
    {
        using namespace retro;

        return (handle.block << (32 - NAME_MAX_BLOCK_BITS)) + handle.block // Let block index impact most hash bits
               + (handle.offset << NAME_BLOCK_OFFSET_BITS) + handle.offset // Let offset impact most hash bits
               + (handle.offset >> 4); // Reduce impact of non-uniformly distributed entry name lengths
    }
};

size_t std::hash<retro::NameEntryId>::operator()(const retro::NameEntryId name) const noexcept
{
    return std::hash<retro::NameEntryHandle>{}(name);
}

namespace retro
{

    template <uint32 NumBits>
    class NameAtomicBitSet
    {
        using WordType = uint64;
        static constexpr uint32 BITS_PER_WORD = 8 * sizeof(WordType);
        static constexpr uint32 NUM_WORDS = NumBits / BITS_PER_WORD;
        static constexpr uint32 BIT_INDEX_MASK = BITS_PER_WORD - 1;

        static_assert((NumBits % BITS_PER_WORD) == 0);

      public:
        bool set_bit_atomic(uint32 index)
        {
            const uint32 word_index = index / BITS_PER_WORD;
            const uint32 bit_index = index & BIT_INDEX_MASK;
            const WordType value_mask = static_cast<WordType>(1) << bit_index;
            volatile WordType *dest_dword = get_buffer() + word_index;
            assert(dest_dword >= get_buffer() && dest_dword < (get_buffer() + NUM_WORDS));
            const WordType prev_dword_value =
                std::atomic_fetch_or(std::bit_cast<std::atomic<WordType> *>(dest_dword), value_mask);
            return prev_dword_value & value_mask;
        }

        bool clear_bit_atomic(uint32 index)
        {
            const uint32 word_index = index / BITS_PER_WORD;
            const uint32 bit_index = index & BIT_INDEX_MASK;
            const WordType value_mask = ~(static_cast<WordType>(1) << bit_index);
            volatile WordType *dest_dword = get_buffer() + word_index;
            assert(dest_dword >= get_buffer() && dest_dword < (get_buffer() + NUM_WORDS));
            const WordType prev_dword_value =
                std::atomic_fetch_and(std::bit_cast<std::atomic<WordType> *>(dest_dword), ~value_mask);
            return prev_dword_value & value_mask;
        }

        template <typename Functor>
            requires std::invocable<Functor, WordType>
        void for_each_set_bit(Functor func) const
        {
            const WordType *words = data_.load(std::memory_order_relaxed);
            if (words == nullptr)
                return;

            uint32 word_index = 0;
            for (std::atomic atomic_word : std::span{words, NUM_WORDS})
            {
                const WordType word = atomic_word.load();
                for (uint32 bit_index = 0; bit_index < BITS_PER_WORD; ++bit_index)
                {
                    if ((word & static_cast<WordType>(1) << bit_index) != 0)
                    {
                        func(word_index * BITS_PER_WORD + bit_index);
                    }
                }

                ++word_index;
            }
        }

      private:
        WordType *get_buffer()
        {
            WordType *buffer = data_.load(std::memory_order_relaxed);
            if (buffer == nullptr)
            {
                constexpr uint32 buffer_size_bytes = NUM_WORDS * sizeof(WordType);
                buffer = static_cast<WordType *>(std::malloc(buffer_size_bytes));
                std::memset(buffer, 0, buffer_size_bytes);
                WordType *expected = nullptr;
                if (data_.compare_exchange_strong(expected, buffer)) [[unlikely]]
                {
                    std::free(expected);
                    buffer = expected;
                }
            }

            return buffer;
        }

        std::atomic<WordType *> data_{};
    };

    struct NameSlot
    {
        static constexpr uint32 ENTRY_ID_BITS = NAME_MAX_BLOCK_BITS + NAME_BLOCK_OFFSET_BITS;
        static constexpr uint32 ENTRY_ID_MASK = (1 << ENTRY_ID_BITS) - 1;
        static constexpr uint32 PROBE_HASH_SHIFT = ENTRY_ID_BITS;
        static constexpr uint32 PROBE_HASH_MASK = ~ENTRY_ID_MASK;

        NameSlot() = default;

        NameSlot(NameEntryId value, uint32 probe_hash) : id_and_hash_{value.to_unstable_int() | probe_hash}
        {
            assert(!(value.to_unstable_int() & PROBE_HASH_MASK) && !(probe_hash & ENTRY_ID_MASK) && used());
        }

        [[nodiscard]] NameEntryId id() const
        {
            return NameEntryId::from_unstable_int(id_and_hash_ & ENTRY_ID_MASK);
        }

        [[nodiscard]] uint32 probe_hash() const
        {
            return id_and_hash_ >> PROBE_HASH_MASK;
        }

        [[nodiscard]] friend bool operator==(const NameSlot &lhs, const NameSlot &rhs) noexcept
        {
            return lhs.id_and_hash_ == rhs.id_and_hash_;
        }

        [[nodiscard]] bool used() const
        {
            return id_and_hash_ != 0;
        }

      private:
        uint32 id_and_hash_ = 0;
    };

    class NameEntryAllocator
    {
      public:
        static constexpr usize STRIDE = alignof(NameEntry);
        static constexpr usize BLOCK_SIZE_BYTES = STRIDE * NAME_BLOCK_OFFSETS;

        NameEntryAllocator()
        {
            blocks_[0] = alloc_block();
        }

        ~NameEntryAllocator()
        {
            for (int32 index = current_block_; index >= 0; --index)
            {
                std::free(blocks_[index]);
            }
        }

        void reserve_blocks(uint32 num)
        {
            std::unique_lock _{lock_};

            for (uint32 index = num - 1; index > current_block_ && blocks_[index] == nullptr; --index)
            {
                blocks_[index] = alloc_block();
            }
        }

        NameEntryHandle allocate(uint32 bytes)
        {
            assert(current_byte_cursor_ & STRIDE == 0);
            assert(current_byte_cursor_ + bytes <= BLOCK_SIZE_BYTES);

            uint32 byte_offset = current_byte_cursor_;
            uint32 step = align(bytes, alignof(NameEntry));
            waste_ += step - bytes;
            current_byte_cursor_ += step;
            return NameEntryHandle{current_block_, static_cast<uint32>(byte_offset / STRIDE)};
        }

        template <typename ScopeLock>
        NameEntryHandle allocate_regular(NameStringView name)
        {
            ScopeLock _{lock_};

            uint32 bytes = BLOCK_SIZE_BYTES < current_byte_cursor_
                               ? 0
                               : NameStore::try_place(blocks_[current_block_] + current_byte_cursor_,
                                                      BLOCK_SIZE_BYTES - current_byte_cursor_,
                                                      name);

            if (bytes == 0)
            {
                allocate_new_block();
                bytes = NameStore::try_place(blocks_[current_block_] + current_byte_cursor_,
                                             BLOCK_SIZE_BYTES - current_byte_cursor_,
                                             name);
                assert(bytes != 0);
            }

            return allocate(bytes);
        }

        template <typename ScopeLock>
        NameEntryHandle create(NameStringView name, boost::optional<NameEntryId> comparisonId, NameEntryHeader header)
        {
            prefetch(blocks_[current_block_]);

            auto handle = allocate_regular<ScopeLock>(name);
            auto &entry = resolve(handle);

#if RETRO_WITH_CASE_PRESERVING_NAME
            entry.comparison_id = comparisonId.value_or(handle);
#endif

            entry.header = header;

            NameStore::finalize(entry.name_data, name);

            return handle;
        }

        NameEntry &resolve(NameEntryHandle handle) const
        {
            return *std::bit_cast<NameEntry *>(blocks_[handle.block] + STRIDE + handle.offset);
        }

        void batch_lock() const
        {
            lock_.lock();
        }

        void batch_unlock() const
        {
            lock_.unlock();
        }

        int32 num_blocks() const
        {
            return current_block_ + 1;
        }

        int64 waste() const
        {
            return waste_;
        }

        std::byte **blocks_for_debug_visualizer()
        {
            return blocks_.data();
        }

        [[nodiscard]] std::vector<const NameEntry *> dump_debug() const
        {
            std::shared_lock _{lock_};

            std::vector<const NameEntry *> result;
            for (uint32 block_index = 0; block_index < current_block_; ++block_index)
            {
                debug_dump_block(blocks_[block_index], BLOCK_SIZE_BYTES, result);
            }

            debug_dump_block(blocks_[current_block_], current_byte_cursor_, result);
            return result;
        }

      private:
        static void debug_dump_block(std::byte *block, uint32 block_size, std::vector<const NameEntry *> &result)
        {
            const std::byte *it = block;
            const std::byte *end = it + block_size - NameEntry::data_offset();
            while (it < end)
            {
                auto *entry = std::bit_cast<const NameEntry *>(it);
                if (uint32 length = entry->header_.length; length > 0)
                {
                    result.push_back(entry);
                    it += entry->size_in_bytes();
                }
                else
                {
                    break;
                }
            }
        }

        static std::byte *alloc_block()
        {
            return static_cast<std::byte *>(std::malloc(BLOCK_SIZE_BYTES));
        }

        void allocate_new_block()
        {
            if (current_byte_cursor_ + NameEntry::data_offset() <= BLOCK_SIZE_BYTES)
            {
                auto *terminator = std::bit_cast<NameEntry *>(blocks_[current_block_] + current_byte_cursor_);
                terminator->header_.length = 0;
            }

            if (current_byte_cursor_ < BLOCK_SIZE_BYTES)
            {
                waste_ += (BLOCK_SIZE_BYTES - current_byte_cursor_);
            }

            ++current_block_;
            current_byte_cursor_ = 0;

            assert(current_block_ < NAME_MAX_BLOCKS);

            if (blocks_[current_block_] == nullptr)
            {
                blocks_[current_block_] = alloc_block();
            }

            prefetch(blocks_[current_block_]);
        }

        mutable std::shared_mutex lock_;
        uint32 current_block_ = 0;
        uint32 current_byte_cursor_ = 0;
        std::array<std::byte *, NAME_MAX_BLOCKS> blocks_;
        int64 waste_ = 0;
    };

    class BatchLockGuard
    {
      public:
        explicit BatchLockGuard(NameEntryAllocator &allocator) : allocator_{allocator}
        {
            allocator_.batch_lock();
        }

        BatchLockGuard(const BatchLockGuard &) = delete;
        BatchLockGuard(BatchLockGuard &&) = delete;

        ~BatchLockGuard()
        {
            allocator_.batch_unlock();
        }
        BatchLockGuard &operator=(const BatchLockGuard &) = delete;
        BatchLockGuard &operator=(BatchLockGuard &&) = delete;

      private:
        NameEntryAllocator &allocator_;
    };

#if RETRO_WITH_CASE_PRESERVING_NAME
    constexpr uint32 NAME_POOL_SHARD_BITS = 10;
#else
    constexpr uint32 NAME_POOL_SHARD_BITS = 8;
#endif

    constexpr uint32 NAME_POOL_SHARDS = 1 << NAME_POOL_SHARD_BITS;
    constexpr uint32 NAME_POOL_INITIAL_SLOT_BITS = 8;
    constexpr uint32 NAME_POOL_INITIAL_SLOTS_PER_SHARD = 1 << NAME_POOL_INITIAL_SLOT_BITS;

    struct NameHash
    {
        uint32 shard_index;
        uint32 unmasked_slot_index;
        uint32 slot_probe_hash;
        NameEntryHeader entry_probe_header;

        static constexpr uint64 ALGORITHM_ID = 0xC1640000;
        static constexpr uint32 SHARD_MASK = NAME_POOL_SHARDS - 1;

        static uint32 get_shard_index(const uint64 hash)
        {
            return static_cast<uint32>(hash >> 32) & SHARD_MASK;
        }

        template <Char CharType>
        static uint64 generate_hash(const CharType *str, int32 length)
        {
            return city_hash_64(std::bit_cast<const char *>(str), str, length);
        }

        static uint64 generate_hash(NameEntryId string_part, int32 number_part)
        {
            return Hash128to64({number_part, string_part.to_unstable_int()});
        }

        template <Char CharType>
        NameHash(const CharType *str, int32 length)
            : NameHash(generate_hash(str, length),
                       length,
                       is_utf8_none(str, length),
                       sizeof(CharType) == sizeof(char16_t))
        {
        }

        template <Char CharType>
        NameHash(const CharType *str, int32 length, int64 hash)
            : NameHash(hash, length, is_utf8_none(str, length), sizeof(CharType) == sizeof(char16_t))
        {
        }

        NameHash(NameEntryId string_part, int32 number_part)
            : NameHash(generate_hash(string_part, number_part), 0, false, false)
        {
        }

        NameHash(uint64 hash, int32 length, bool is_none, bool is_utf16)
        {
            uint32 hi = static_cast<uint32>(hash >> 32);
            uint32 lo = static_cast<uint32>(hash);

            uint32 is_none_bit = is_none << NameSlot::PROBE_HASH_SHIFT;

            static_assert((SHARD_MASK & NameSlot::PROBE_HASH_MASK) == 0);

            shard_index = hi & SHARD_MASK;
            unmasked_slot_index = lo;
            slot_probe_hash = (hi & NameSlot::PROBE_HASH_MASK) | is_none_bit;
            entry_probe_header.length = length;
            entry_probe_header.is_utf16 = is_utf16;

#if !RETRO_WITH_CASE_PRESERVING_NAME
            static constexpr uint32 ENTRY_PROBE_MASK = (1u << NameEntryHeader::PROBE_HASH_BITS) - 1;
            entry_probe_header.lowercase_probe_hash =
                static_cast<uint16>((hi >> NAME_POOL_SHARD_BITS) & ENTRY_PROBE_MASK)
#endif
        }

        [[nodiscard]] uint32 get_probe_start(uint32 slot_mask) const
        {
            return unmasked_slot_index & slot_mask;
        }

        static uint32 get_probe_start(const uint32 unmasked_slot_index, const uint32 slot_mask)
        {
            return unmasked_slot_index & slot_mask;
        }

        static bool is_utf8_none(const char16_t *, int32)
        {
            return false;
        }

        static uint32 is_utf8_none(const char *str, int32 length)
        {
            if (length != 4)
            {
                return false;
            }

            static constexpr uint32 NONE_AS_INT = std::endian::native == std::endian::little ? 0x454e4f4e : 0x4e4f4e45;
            static constexpr uint32 TO_UPPER_MASK = 0xdfdfdfdf;

            const uint32 four_chars = read_unaligned<uint32>(str);
            return (four_chars & TO_UPPER_MASK) == NONE_AS_INT;
        }

        friend bool operator==(const NameHash &lhs, const NameHash &rhs) noexcept
        {
            return lhs.shard_index == rhs.shard_index && lhs.unmasked_slot_index == rhs.unmasked_slot_index &&
                   lhs.slot_probe_hash == rhs.slot_probe_hash && lhs.entry_probe_header == rhs.entry_probe_header;
        }
    };

    template <Char CharType>
    constexpr uint64 generate_lower_case_hash(const CharType *str, const int32 length)
    {
        std::array<CharType, NAME_SIZE> lower_str;
        if constexpr (std::same_as<CharType, char16_t>)
        {
            una::cases::to_casefold_utf16(std::u16string_view(str, length), FixedBufferAllocator{lower_str});
        }
        else
        {
            una::cases::to_casefold_utf8(std::string_view(str, length), FixedBufferAllocator{lower_str});
        }

        return NameHash::generate_hash(lower_str.data(), length);
    }

    static uint64 generate_lower_case_hash(NameStringView name)
    {
        return name.is_utf16 ? generate_lower_case_hash(name.utf16, name.length)
                             : generate_lower_case_hash(name.utf8, name.length);
    }

    template <Char CharType>
    NameHash hash_lower_case(const CharType *str, uint32 length)
    {
        std::array<CharType, NAME_SIZE> lower_str;
        if constexpr (std::same_as<CharType, char16_t>)
        {
            una::cases::to_casefold_utf16(std::u16string_view(str, length), FixedBufferAllocator{lower_str});
        }
        else
        {
            una::cases::to_casefold_utf8(std::string_view(str, length), FixedBufferAllocator{lower_str});
        }
        return NameHash{lower_str.data(), static_cast<int32>(length)};
    }

    template <NameCase Sensitivity>
    NameHash hash_name(NameStringView name)
    {
        if constexpr (Sensitivity == NameCase::IgnoreCase)
        {
            return name.is_utf8 ? hash_lower_case(name.utf8, name.length) : hash_lower_case(name.utf16, name.length);
        }
        else
        {
            return name.is_utf8 ? NameHash{name.utf8, static_cast<int32>(name.length)}
                                : NameHash{name.utf16, static_cast<int32>(name.length)};
        }
    }

    bool NameStringView::is_none_string() const
    {
        return is_utf8 ? NameHash::is_utf8_none(utf8, length) : NameHash::is_utf8_none(utf16, length);
    }

    template <NameCase Sensitivity>
    struct NameValue
    {
        explicit NameValue(NameStringView name) : name(name), hash(hash_name<Sensitivity>(name))
        {
        }

        NameValue(NameStringView name, NameHash hash) : name(name), hash(hash)
        {
        }

        NameValue(NameStringView name, uint64 hash)
            : name(name),
              hash(name.is_utf8 ? NameHash{name.utf16, static_cast<int32>(name.length), static_cast<int64>(hash)}
                                : NameHash{name.utf16, static_cast<int32>(name.length), static_cast<int64>(hash)})
        {
        }

        NameStringView name;
        NameHash hash;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameEntryId comparison_id;
#endif
    };

    using NameComparisonValue = NameValue<NameCase::IgnoreCase>;
#if RETRO_WITH_CASE_PRESERVING_NAME
    using NameDisplayValue = NameValue<NameCase::CaseSensitive>;
#endif

    constexpr boost::optional<NameEntryId> get_existing_comparison_id(NameComparisonValue)
    {
        return boost::none;
    }

#if RETRO_WITH_CASE_PRESERVING_NAME
    constexpr boost::optional<NameEntryId> get_existing_comparison_id(const NameDisplayValue &value)
    {
        return value.comparison_id;
    }
#endif

    struct NullScopeLock
    {
        explicit NullScopeLock(std::shared_mutex &)
        {
        }
    };

    template <NameCase Sensitivity>
    struct NameLoad
    {
        NameValue<Sensitivity> in;
        DisplayNameEntryId *out = nullptr;
        bool in_reuse_comparison_entry = false;
        bool out_create_new_entry = false;

        void set_out(NameEntryId id)
        {
            if constexpr (Sensitivity == NameCase::IgnoreCase)
            {
                out->set_loaded_comparison_id(id);
            }
            else
            {
#if RETRO_WITH_CASE_PRESERVING_NAME
                out->set_loaded_different_display_id(id);
#else
                static_assert(false, "Case sensitive name comparison requires RETRO_WITH_CASE_PRESERVING_NAME");
#endif
            }
        }
    };

    using NameComparisonLoad = NameLoad<NameCase::IgnoreCase>;
#if RETRO_WITH_CASE_PRESERVING_NAME
    using NameDisplayLoad = NameLoad<NameCase::CaseSensitive>;
#endif

    class NamePoolShardBase
    {
      public:
        void initialize(NameEntryAllocator &entries)
        {
            entries_ = &entries;

            slots_ = static_cast<NameSlot *>(std::malloc(NAME_POOL_INITIAL_SLOTS_PER_SHARD * sizeof(NameSlot)));
            std::memset(slots_, 0, NAME_POOL_INITIAL_SLOTS_PER_SHARD * sizeof(NameSlot));
            capacity_mask_ = NAME_POOL_INITIAL_SLOTS_PER_SHARD - 1;
        }

        ~NamePoolShardBase()
        {
            std::free(slots_);
            used_slots_ = 0;
            capacity_mask_ = 0;
            slots_ = nullptr;
            num_created_entries_ = 0;
            num_created_utf16_entries_ = 0;
        }

        uint32 capacity() const
        {
            return capacity_mask_ + 1;
        }

        uint32 num_created() const
        {
            return num_created_entries_.load(std::memory_order_relaxed);
        }

        uint32 num_created_utf16() const
        {
            return num_created_utf16_entries_.load(std::memory_order_relaxed);
        }

      protected:
        static constexpr uint32 LOAD_FACTOR_QUOTIENT = 9;
        static constexpr uint32 LOAD_FACTOR_DIVISOR = 10;

        mutable std::shared_mutex lock_;
        uint32 used_slots_ = 0;
        uint32 capacity_mask_ = 0;
        NameSlot *slots_ = nullptr;
        NameEntryAllocator *entries_ = nullptr;
        std::atomic<uint32> num_created_entries_{0};
        std::atomic<uint32> num_created_utf16_entries_{0};

        template <NameCase Sensitivity>
        static bool entry_equals_value(NameEntry entry, const NameValue<Sensitivity> &value)
        {
            return entry.header_ == value.hash.entry_probe_header &&
                   equals_same_dimensions<Sensitivity>(entry, value.name);
        }
    };

    template <NameCase Sensitivity>
    class NamePoolShard : public NamePoolShardBase
    {
      public:
        NameEntryId find(NameValue<Sensitivity> value)
        {
            std::shared_lock _{lock_};
            auto &slot = probe(value);
            return slot.id();
        }

        template <typename ScopeLock = std::unique_lock>
        std::pair<NameEntryId, bool> insert(const NameValue<Sensitivity> &value)
        {
            ScopeLock _{lock_};
            auto &slot = probe(value);

            if (slot.used())
            {
                return {slot.id(), false};
            }

            return {create_and_insert_entry<ScopeLock>(value), true};
        }

        void insert_existing_entry(NameHash hash, NameEntryId existing_id)
        {
            insert_existing_entry_impl<std::unique_lock>(hash, existing_id);
        }

      private:
        template <typename ShardScopeLock>
        void insert_existing_entry_impl(NameHash hash, NameEntryId existing_id)
        {
            const NameSlot new_lookup{existing_id, hash.slot_probe_hash};

            ShardScopeLock _{lock_};

            auto &slot = probe(hash.unmasked_slot_index, [=](const NameSlot old) { return old == new_lookup; });

            if (!slot.used())
            {
                claim_slot(slot, new_lookup);
            }
        }

      public:
        void insert_batch(std::span<NameLoad<Sensitivity>> batch)
        {
            if (batch.empty())
            {
                return;
            }

            uint32 probe_look_ahead = static_cast<uint32>(std::min(batch.size() - 1, 4));

            std::unique_lock _{lock_};

            for (uint32 batch_index = 0; batch_index < probe_look_ahead; ++batch_index)
            {
                probe_prefetch(batch[batch].in);
            }

            auto *prefetch_it = batch.data() + probe_look_ahead;

            uint32 num_new_slots = 0;
            for (auto &request : batch)
            {
                if constexpr (Sensitivity == NameCase::IgnoreCase)
                {
                    if (request.in_reuse_comparison_entry)
                    {
                        assert(*request.out == get_existing_comparison_id(request.in).value());
                        request.out_create_new_entry = false;
                        ++num_new_slots;
                        continue;
                    }
                }

                probe_prefetch(prefetch_it->in);
                if (prefetch_it != &batch.back())
                {
                    ++prefetch_it;
                }

                auto slot = probe(request.in);
                request.set_out(slot.id());
                request.out_create_new_entry = !slot.used();
                num_new_slots += slot.used();
            }

            if (num_new_slots > 0)
            {
                reserve_impl<NullScopeLock>(used_slots_ + num_new_slots);

                BatchLockGuard batch_lock_guard{*entries_};
                for (auto &request : batch)
                {
                    prefetch(request.in.name.data);

                    if constexpr (Sensitivity == NameCase::IgnoreCase)
                    {
                        if (request.in_reuse_comparison_entry)
                        {
                            insert_existing_entry_impl<NullScopeLock>(request.in.hash,
                                                                      get_existing_comparison_id(request.in).value());
                            continue;
                        }
                    }

                    if (request.out_create_new_entry)
                    {
                        auto &slot = probe(request.in);
                        request.out_create_new_entry = !slot.used();
                        request.set_out(slot.used() ? slot.id() : create_and_insert_entry<NullScopeLock>(request.in));
                    }
                }
            }
        }

        void reserve(uint32 number)
        {
            reserve_impl<std::unique_lock>(number);
        }

      private:
        template <typename SharedScopeLock>
        void reserve_impl(uint32 number)
        {
            uint32 wanted_capacity =
                round_up_to_power_of_two((number + number) * LOAD_FACTOR_DIVISOR / LOAD_FACTOR_QUOTIENT);

            SharedScopeLock _{lock_};
            if (wanted_capacity > capacity())
            {
                grow(wanted_capacity);
            }
        }

        void claim_slot(NameSlot &unused_slot, NameSlot new_value)
        {
            assert(!unused_slot.used());
            unused_slot = new_value;

            ++used_slots_;
            if (used_slots_ * LOAD_FACTOR_DIVISOR > LOAD_FACTOR_QUOTIENT * capacity())
            {
                grow();
            }
        }

        template <typename EntryScopeLock>
        NameEntryId create_and_insert_entry(NameSlot &slot, const NameValue<Sensitivity> &value)
        {
            auto new_entry_id = entries_->create<EntryScopeLock>(value.name,
                                                                 get_existing_comparison_id(value),
                                                                 value.hash.entry_probe_header);
            claim_slot(slot, NameSlot{new_entry_id, value.hash.slot_probe_hash});

            num_created_entries_.fetch_add(1, std::memory_order_relaxed);
            if (value.name.is_utf16)
            {
                num_created_utf16_entries_.fetch_add(1, std::memory_order_relaxed);
            }

            return new_entry_id;
        }

        void grow()
        {
            grow(capacity() * 2);
        }

        void grow(const uint32 new_capacity) noexcept
        {
            std::span old_slots{slots_, capacity()};
            const uint32 old_used_slots = used_slots_;

            slots_ = static_cast<NameSlot *>(std::malloc(new_capacity * sizeof(NameSlot)));
            std::memset(slots_, 0, new_capacity * sizeof(NameSlot));
            used_slots_ = 0;
            capacity_mask_ = new_capacity - 1;

            constexpr uint32 prefetch_depth = 8;
            std::array<NameSlot, prefetch_depth> prefetch_slots;
            uint32 num_prefetched = 0;

            for (auto old_slot : old_slots)
            {
                if (old_slot.used())
                {
                    prefetch(&entries_->resolve(old_slot.id()));

                    ++num_prefetched;
                    if (num_prefetched == prefetch_depth)
                    {
                        for (auto prefetched_slot : prefetch_slots)
                        {
                            rehash_and_insert(prefetched_slot);
                        }

                        num_prefetched = 0;
                    }
                }
            }

            for (auto prefetched_slot : std::span{prefetch_slots}.first(num_prefetched))
            {
                rehash_and_insert(prefetched_slot);
            }

            assert(old_used_slots == used_slots_);

            std::free(old_slots.data());
        }

        void prove_prefetch(const NameValue<Sensitivity> &value)
        {
            prefetch(value.name.data);
            prefetch(slots_ + NameHash::get_probe_start(value.hash.unmasked_slot_index, capacity_mask_));
        }

        NameSlot &probe(const NameValue<Sensitivity> &value)
        {
            return probe(value.hash.unmasked_slot_index,
                         [&](NameSlot slot)
                         {
                             return slot.probe_hash() == value.hash.slot_probe_hash &&
                                    entry_equals_value<Sensitivity>(entries_->resolve(slot.id(), value));
                         });
        }

        template <std::invocable<NameSlot> Predicate>
            requires std::convertible_to<std::invoke_result_t<Predicate, NameSlot>, bool>
        NameSlot &probe(uint32 unmasked_slot_index, Predicate predicate)
        {
            const uint32 mask = capacity_mask_;
            for (uint32 i = NameHash::get_probe_start(unmasked_slot_index, mask); true; i = (i + 1) & mask)
            {
                if (auto &slot = slots_[i]; !slot.used() || predicate(slot))
                {
                    return slot;
                }
            }
        }

        void rehash_and_insert(NameSlot old_slot)
        {
            assert(old_slot.used());

            const auto &entry = entries_->resolve(old_slot.id());

            NameBuffer decode_buffer;
            auto name = entry.make_view(decode_buffer);
            auto hash = hash_name<Sensitivity>(name);
            auto &new_slot = probe(hash.unmasked_slot_index, [](NameSlot) { return false; });
            new_slot = old_slot;
            ++used_slots_;
        }
    };

    class NamePool
    {
      public:
        NamePool()
        {
            for (auto &shard : comparison_shards_)
            {
                shard.initialize(entries_);
            }

#if RETRO_WITH_CASE_PRESERVING_NAME
            for (auto &shard : display_shards_)
            {
                shard.initialize(entries_);
            }
#endif
        }

        void reserve(uint32 num_blocks, uint32 num_entries);
        NameEntryId store(NameStringView name);

        NameEntryId find(const NameStringView name)
        {
#if RETRO_WITH_CASE_PRESERVING_NAME
            const NameDisplayValue display_value{name};
            if (const auto existing = display_shards_[display_value.hash.shard_index].find(display_value);
                existing.is_valid())
            {
                return existing;
            }
#endif

            const NameComparisonValue comparison_value{name};
            return comparison_shards_[comparison_value.hash.shard_index].find(comparison_value);
        }

        [[nodiscard]] NameEntry &resolve(NameEntryHandle handle) const
        {
            return entries_.resolve(handle);
        }

        bool is_valid(NameEntryHandle handle) const;

        DisplayNameEntryId store_value(const NameComparisonValue &value);
        void store_batch(uint32 shard_index, std::span<NameComparisonLoad> batch)
        {
            comparison_shards_[shard_index].insert_batch(batch);
        }

#if RETRO_WITH_CASE_PRESERVING_NAME
        DisplayNameEntryId store_value(const NameDisplayValue &value, bool resuse_comparison_id);
        void store_batch(uint32 shard_index, std::span<NameDisplayLoad> batch)
        {
            display_shards_[shard_index].insert_batch(batch);
        }
        bool resuse_comparison_entry(bool added_comparison_entry, const NameDisplayValue &display_value);
#endif

        [[nodiscard]] uint32 num_entries() const;
        [[nodiscard]] uint32 num_utf8_entries() const;
        [[nodiscard]] uint32 num_utf16_entries() const;
        [[nodiscard]] uint32 num_blocks() const
        {
            return entries_.num_blocks();
        }
        uint32 num_slots() const;

        std::byte **blocks_for_debug_visualizer()
        {
            return entries_.blocks_for_debug_visualizer();
        }

        std::vector<const NameEntry *> debug_dump() const;

      private:
        NameEntryAllocator entries_;
#if RETRO_WITH_CASE_PRESERVING_NAME
        std::array<NamePoolShard<NameCase::CaseSensitive>, NAME_POOL_SHARDS> display_shards_;
#endif
        std::array<NamePoolShard<NameCase::IgnoreCase>, NAME_POOL_SHARDS> comparison_shards_;
    };

    template <NameCase SearchCase>
    static std::strong_ordering compare_different_ids_alphabetically(NameEntryId a, NameEntryId b)
    {
        assert(a != b);
    }

    std::strong_ordering NameEntryId::compare_lexical(NameEntryId other) const
    {
        return value_ != other.value_ ? compare_different_ids_alphabetically<NameCase::IgnoreCase>(*this, other)
                                      : std::strong_ordering::equal;
    }

    std::strong_ordering NameEntryId::compare_lexical_case_sensitive(NameEntryId other) const
    {
        return value_ != other.value_ ? compare_different_ids_alphabetically<NameCase::CaseSensitive>(*this, other)
                                      : std::strong_ordering::equal;
    }
} // namespace retro
