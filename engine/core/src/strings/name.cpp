/**
 * @file name.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/unordered/unordered_flat_map.hpp>
#include <cassert>

module retro.core.strings.name;

import retro.core.containers.optional;

namespace retro
{
    namespace
    {
        constexpr StringComparison to_string_comparison(const NameCase name_case)
        {
            switch (name_case)
            {
                using enum NameCase;
                case case_sensitive:
                    return StringComparison::case_sensitive;
                case ignore_case:
                    return StringComparison::case_insensitive;
            }

            return StringComparison::case_insensitive;
        }
    } // namespace

    struct NameHash
    {
        std::size_t hash{};
        std::uint32_t length{};

        friend bool operator==(NameHash lhs, NameHash rhs) = default;
    };

    constexpr std::size_t hash_value(const NameHash &name) noexcept
    {
        return hash_combine(name.hash, name.length);
    }

    template <NameCase CaseSensitivity>
        requires(CaseSensitivity == NameCase::case_sensitive || CaseSensitivity == NameCase::ignore_case)
    struct NameEntryComparer
    {
        static NameHash hash(const std::string_view name)
        {
            if constexpr (CaseSensitivity == NameCase::case_sensitive)
            {
                return NameHash{std::hash<std::string_view>{}(name), static_cast<std::uint32_t>(name.size())};
            }
            else
            {
                InlineArena<NAME_INLINE_BUFFER_SIZE> arena;
                const auto as_lower = to_lower(name, make_allocator<char>(arena));
                return NameHash{std::hash<std::string_view>{}(as_lower), static_cast<std::uint32_t>(name.size())};
            }
        }

        static std::strong_ordering compare(const std::string_view a, const std::string_view b)
        {
            return retro::compare<to_string_comparison(CaseSensitivity)>(a, b);
        }
    };

    template <NameCase CaseSensitivity>
    class NameTableSet
    {
      public:
        using Comparer = NameEntryComparer<CaseSensitivity>;

        [[nodiscard]] Optional<NameEntryId> find(std::string_view str)
        {
            const NameHash hash = Comparer::hash(str);
            if (const auto existing = entry_indexes_.find(hash); existing != entry_indexes_.end())
            {
                return existing->second;
            }

            return std::nullopt;
        }

        template <typename Factory>
            requires std::invocable<Factory, std::string_view>
        NameEntryId find_or_add(std::string_view str, Factory &&factory)
        {
            const NameHash hash = Comparer::hash(str);
            if (const auto existing = entry_indexes_.find(hash); existing != entry_indexes_.end())
            {
                return existing->second;
            }

            const auto new_id = std::forward<Factory>(factory)(str);
            entry_indexes_.emplace(hash, new_id);
            return new_id;
        }

        template <typename Factory>
            requires std::invocable<Factory, std::string_view>
        NameEntryId add(std::string_view str, Factory &&factory)
        {
            const NameHash hash = Comparer::hash(str);
            const auto new_id = std::forward<Factory>(factory)(str);
            entry_indexes_.emplace(hash, new_id);
            return new_id;
        }

      private:
        boost::unordered_flat_map<NameHash, NameEntryId> entry_indexes_;
    };

    class NameAllocator
    {
      public:
        template <typename T, typename... Args>
            requires std::constructible_from<T, Args...> && std::is_trivially_destructible_v<T>
        constexpr decltype(auto) allocate_with_tail(const std::size_t tail_size, Args &&...args)
        {
            const std::size_t total_size = sizeof(T) + tail_size;
            constexpr std::size_t alignment = alignof(T);

            auto *ptr = static_cast<T *>(arena_.allocate(total_size, alignment));
            std::construct_at(ptr, std::forward<Args>(args)...);

            return *ptr;
        }

      private:
        static constexpr std::size_t BLOCK_SIZE = 1024 * 64;
        static constexpr std::size_t INITIAL_BLOCKS = 16;
        static constexpr std::size_t MAX_BLOCKS = 1024;

        MultiArena arena_{BLOCK_SIZE, INITIAL_BLOCKS, MAX_BLOCKS};
    };

    class NameTable
    {
        NameTable()
        {
            get_or_add_entry_internal(NONE_STRING, FindType::add);
        }

      public:
        static NameTable &instance()
        {
            static NameTable instance;
            return instance;
        }

        NameIndices get_or_add_entry(const std::string_view str, const FindType find_type)
        {

            if (retro::compare<StringComparison::case_insensitive>(str, NONE_STRING) == std::strong_ordering::equal)
            {
                return NameIndices
                {
                    .comparison_index = NameEntryId::none(),
#if RETRO_WITH_CASE_PRESERVING_NAME
                    .display_index = NameEntryId::none()
#endif
                };
            }

            return get_or_add_entry_internal(str, find_type);
        }

        [[nodiscard]] std::string_view get(const NameEntryId id) const
        {
            if (id.is_none())
            {
                return NONE_STRING;
            }

            std::shared_lock lock{mutex_};
            auto &entry = *entries_[id.value()];
            return entry.name();
        }

        template <NameCase CaseSensitivity>
            requires(CaseSensitivity == NameCase::case_sensitive || CaseSensitivity == NameCase::ignore_case)
        [[nodiscard]] std::strong_ordering compare(const NameEntryId lhs, const NameEntryId rhs) const noexcept
        {
            return compare<CaseSensitivity>(lhs, get(rhs));
        }

        template <NameCase CaseSensitivity>
            requires(CaseSensitivity == NameCase::case_sensitive || CaseSensitivity == NameCase::ignore_case)
        [[nodiscard]] std::strong_ordering compare(const NameEntryId lhs, std::string_view rhs) const noexcept
        {
            return NameEntryComparer<CaseSensitivity>::compare(get(lhs), rhs);
        }

        [[nodiscard]] bool is_within_bounds(const NameEntryId index) const noexcept
        {
            std::shared_lock lock{mutex_};
            return index.value() < entries_.size();
        }

        [[nodiscard]] const std::vector<const NameEntry *> &entries() const noexcept
        {
            return entries_;
        }

      private:
        NameIndices get_or_add_entry_internal(std::string_view str, const FindType find_type)
        {

            if (find_type == FindType::add)
            {
                std::unique_lock lock{mutex_};
                const auto next_index = static_cast<std::uint32_t>(entries_.size());
                const auto comparison_index =
                    comparison_entries_.find_or_add(str, std::bind_front(&NameTable::create_new_entry, this));
#if RETRO_WITH_CASE_PRESERVING_NAME
                if (comparison_index.value() == next_index)
                {
                    const auto display_index =
                        display_entries_.add(str, [&comparison_index](std::string_view) { return comparison_index; });
                    assert(comparison_index == display_index);
                    return NameIndices{.comparison_index = comparison_index, .display_index = display_index};
                }

                const auto display_index =
                    display_entries_.find_or_add(str, std::bind_front(&NameTable::create_new_entry, this));
#endif

                return NameIndices
                {
                    .comparison_index = comparison_index,
#if RETRO_WITH_CASE_PRESERVING_NAME
                    .display_index = display_index
#endif
                };
            }

            std::shared_lock lock{mutex_};
            return comparison_entries_.find(str)
                .transform(
                    [&](NameEntryId comparison_index)
                    {
                        return NameIndices
                        {
                            .comparison_index = comparison_index,
#if RETRO_WITH_CASE_PRESERVING_NAME
                            .display_index = display_entries_.find(str).value_or(comparison_index)
#endif
                        };
                    })
                .value_or(NameIndices {
                    .comparison_index = NameEntryId::none(),
#if RETRO_WITH_CASE_PRESERVING_NAME
                    .display_index = NameEntryId::none()
#endif
                });
        }

        NameEntryId create_new_entry(const std::string_view str)
        {
            if (str.size() > MAX_NAME_LENGTH)
                throw std::runtime_error{"Name too long"};

            const std::size_t byte_size = (str.size() + 1) * sizeof(char);
            auto &header = allocator_.allocate_with_tail<NameEntryHeader>(byte_size, str.size());
            auto &entry = *std::launder(reinterpret_cast<NameEntry *>(&header));
            std::memcpy(entry.characters_, str.data(), byte_size);
            entry.characters_[str.size()] = '\0';
            const auto entry_id = NameEntryId{static_cast<std::uint32_t>(entries_.size())};
            entries_.push_back(&entry);
            return entry_id;
        }

        mutable std::shared_mutex mutex_{};
        NameAllocator allocator_;
        NameTableSet<NameCase::ignore_case> comparison_entries_;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameTableSet<NameCase::case_sensitive> display_entries_;
#endif
        std::vector<const NameEntry *> entries_{};
    };

    std::strong_ordering NameEntryId::compare_lexical(const NameEntryId other) const noexcept
    {
        return NameTable::instance().compare<NameCase::ignore_case>(*this, other);
    }

    std::strong_ordering NameEntryId::compare_lexical_case_sensitive(const NameEntryId other) const noexcept
    {
        return NameTable::instance().compare<NameCase::case_sensitive>(*this, other);
    }

    bool operator==(const Name &lhs, const std::string_view rhs)
    {
        const auto [number, new_length] = parse_number_from_name(rhs);
        return NameTable::instance().compare<NameCase::ignore_case>(lhs.comparison_index_, rhs.substr(0, new_length)) ==
                   std::strong_ordering::equal &&
               number == lhs.number_;
    }

    [[nodiscard]] bool operator==(const Name &lhs, std::u16string_view rhs)
    {
        InlineArena<NAME_INLINE_BUFFER_SIZE> arena;
        const auto utf8_str = convert_string<char>(rhs, make_allocator<char>(arena));
        return lhs == utf8_str;
    }

    std::strong_ordering operator<=>(const Name &lhs, const std::string_view rhs)
    {
        const auto [number, new_length] = parse_number_from_name(rhs);
        const auto compareString =
            NameTable::instance().compare<NameCase::ignore_case>(lhs.comparison_index_, rhs.substr(0, new_length));
        if (compareString != std::strong_ordering::equal)
        {
            return compareString;
        }
        return number <=> lhs.number_;
    }

    std::strong_ordering operator<=>(const Name &lhs, const std::u16string_view rhs)
    {
        InlineArena<NAME_INLINE_BUFFER_SIZE> arena;
        const auto utf8_str = convert_string<char>(rhs, make_allocator<char>(arena));
        return lhs <=> utf8_str;
    }

    std::string_view Name::get_base_string() const
    {
        return NameTable::instance().get(display_index());
    }

    bool Name::is_within_bounds(const NameEntryId index)
    {
        return NameTable::instance().is_within_bounds(index);
    }

    // NOLINTNEXTLINE
    Name::LookupResult Name::lookup_name(std::string_view value, const FindType find_type)
    {
        // If the name is too long, just truncate it
        if (value.size() > MAX_NAME_LENGTH)
        {
            value = value.substr(0, MAX_NAME_LENGTH);
        }

        if (value.empty())
        {
            return LookupResult{.indices =
                                    NameIndices{
                                        .comparison_index = NameEntryId::none(),
#if RETRO_WITH_CASE_PRESERVING_NAME
                                        .display_index = NameEntryId::none(),
#endif
                                    },
                                .number = NAME_NO_NUMBER_INTERNAL};
        }

        auto [internal_number, new_length] = parse_number_from_name(value);
        const auto new_slice = value.substr(0, new_length);

        return LookupResult{.indices = NameTable::instance().get_or_add_entry(new_slice, find_type),
                            .number = internal_number};
    }

    const std::vector<const NameEntry *> &debug_get_name_entries()
    {
        return NameTable::instance().entries();
    }
} // namespace retro
