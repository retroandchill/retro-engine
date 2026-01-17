/**
 * @file name.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.core;

import fmt;
import boost;
import uni_algo;

namespace retro
{
    struct NameHash
    {
        usize hash{};
        uint32 length{};

        friend bool operator==(NameHash lhs, NameHash rhs) = default;
    };

    constexpr usize hash_value(const NameHash &name) noexcept
    {
        return hash_combine(name.hash, name.length);
    }

    template <NameCase CaseSensitivity>
        requires(CaseSensitivity == NameCase::CaseSensitive || CaseSensitivity == NameCase::IgnoreCase)
    struct NameEntryComparer
    {
        static NameHash hash(const std::string_view name)
        {
            if constexpr (CaseSensitivity == NameCase::CaseSensitive)
            {
                return NameHash{std::hash<std::string_view>{}(name), static_cast<uint32>(name.size())};
            }
            else
            {
                const auto as_lower = una::cases::to_lowercase_utf8(name, boost::pool_allocator<char>{});
                return NameHash{std::hash<std::string_view>{}(as_lower), static_cast<uint32>(name.size())};
            }
        }

        static std::strong_ordering compare(const std::string_view a, const std::string_view b)
        {
            if constexpr (CaseSensitivity == NameCase::CaseSensitive)
            {
                return std::strong_ordering{static_cast<int8>(una::casesens::compare_utf8(a, b))};
            }
            else
            {
                return std::strong_ordering{static_cast<int8>(una::caseless::compare_utf8(a, b))};
            }
        }
    };

    template <NameCase CaseSensitivity>
    class NameTableSet
    {
      public:
        using Comparer = NameEntryComparer<CaseSensitivity>;

        [[nodiscard]] boost::optional<NameEntryId> find(std::string_view str)
        {
            const NameHash hash = Comparer::hash(str);
            if (const auto existing = entry_indexes_.find(hash); existing != entry_indexes_.end())
            {
                return existing->second;
            }

            return boost::none;
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

    class NameTable
    {
        NameTable()
        {
            get_or_add_entry_internal(NONE_STRING, FindType::Add);
        }

      public:
        static NameTable &instance()
        {
            static NameTable instance;
            return instance;
        }

        NameIndices get_or_add_entry(const std::string_view str, const FindType find_type)
        {
            if (una::caseless::compare_utf8(str, NONE_STRING) == 0)
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
            requires(CaseSensitivity == NameCase::CaseSensitive || CaseSensitivity == NameCase::IgnoreCase)
        [[nodiscard]] std::strong_ordering compare(const NameEntryId lhs, const NameEntryId rhs) const noexcept
        {
            return compare<CaseSensitivity>(lhs, get(rhs));
        }

        template <NameCase CaseSensitivity>
            requires(CaseSensitivity == NameCase::CaseSensitive || CaseSensitivity == NameCase::IgnoreCase)
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

            if (find_type == FindType::Add)
            {
                std::unique_lock lock{mutex_};
                const auto next_index = static_cast<uint32>(entries_.size());
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
                .map(
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

            const usize byte_size = (str.size() + 1) * sizeof(char);
            auto &header = allocator_.allocate_with_tail<NameEntryHeader>(byte_size, str.size());
            auto &entry = *std::launder(reinterpret_cast<NameEntry *>(&header));
            std::memcpy(entry.characters_, str.data(), byte_size);
            entry.characters_[str.size()] = '\0';
            const auto entry_id = NameEntryId{static_cast<uint32>(entries_.size())};
            entries_.push_back(&entry);
            return entry_id;
        }

        static constexpr usize BLOCK_SIZE = 1024 * 64;
        static constexpr usize INITIAL_BLOCKS = 16;
        static constexpr usize MAX_BLOCKS = 1024;

        mutable std::shared_mutex mutex_{};
        SimpleArena allocator_{BLOCK_SIZE, INITIAL_BLOCKS, MAX_BLOCKS};
        NameTableSet<NameCase::IgnoreCase> comparison_entries_;
#if RETRO_WITH_CASE_PRESERVING_NAME
        NameTableSet<NameCase::CaseSensitive> display_entries_;
#endif
        std::vector<const NameEntry *> entries_{};
    };

    std::strong_ordering NameEntryId::compare_lexical(const NameEntryId other) const noexcept
    {
        return NameTable::instance().compare<NameCase::IgnoreCase>(*this, other);
    }

    std::strong_ordering NameEntryId::compare_lexical_case_sensitive(const NameEntryId other) const noexcept
    {
        return NameTable::instance().compare<NameCase::CaseSensitive>(*this, other);
    }

    Name::Name(const std::string_view value, const FindType find_type) : Name(lookup_name(value, find_type))
    {
    }

    Name::Name(const std::u16string_view value, const FindType find_type) : Name(lookup_name(value, find_type))
    {
    }

    Name::Name(const std::string &value, const FindType find_type) : Name(lookup_name(value, find_type))
    {
    }

    Name::Name(const std::u16string &value, const FindType find_type) : Name(lookup_name(value, find_type))
    {
    }

    // NOLINTNEXTLINE
    std::string Name::to_string() const
    {
        // ReSharper disable once CppDFAUnreadVariable
        // ReSharper disable once CppDFAUnusedValue
        const auto baseString = NameTable::instance().get(display_index_);
        if (number_ == NAME_NO_NUMBER_INTERNAL)
        {
            return std::string{baseString};
        }

        return std::format("{}_{}", baseString, name_internal_to_external(number_));
    }

    bool operator==(const Name &lhs, const std::string_view rhs)
    {
        const auto [number, new_length] = parse_number_from_name(rhs);
        return NameTable::instance().compare<NameCase::IgnoreCase>(lhs.comparison_index_, rhs.substr(0, new_length)) ==
                   std::strong_ordering::equal &&
               number == lhs.number_;
    }

    [[nodiscard]] bool operator==(const Name &lhs, std::u16string_view rhs)
    {
        const auto utf8_str = una::utf16to8<char16_t, char>(rhs, boost::pool_allocator<char>{});
        return lhs == utf8_str;
    }

    std::strong_ordering operator<=>(const Name &lhs, const std::string_view rhs)
    {
        const auto [number, new_length] = parse_number_from_name(rhs);
        const auto compareString =
            NameTable::instance().compare<NameCase::IgnoreCase>(lhs.comparison_index_, rhs.substr(0, new_length));
        if (compareString != std::strong_ordering::equal)
        {
            return compareString;
        }
        return number <=> lhs.number_;
    }

    std::strong_ordering operator<=>(const Name &lhs, const std::u16string_view rhs)
    {
        const auto utf8_str = una::utf16to8<char16_t, char>(rhs, boost::pool_allocator<char>{});
        return lhs <=> utf8_str;
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

    Name::LookupResult Name::lookup_name(const std::u16string_view value, const FindType find_type)
    {
        const auto utf8_str = una::utf16to8<char16_t, char>(value, boost::pool_allocator<char>{});
        return lookup_name(utf8_str, find_type);
    }

    const std::vector<const NameEntry *> &debug_get_name_entries()
    {
        return NameTable::instance().entries();
    }
} // namespace retro
