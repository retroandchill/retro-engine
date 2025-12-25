//
// Created by fcors on 12/24/2025.
//
module;

#include "retro/core/exports.h"

export module retro.core.strings:name;

import retro.core;
import :comparison;
import :concepts;

namespace retro::core {
    export enum class FindType : uint8 {
        Find,
        Add
    };

    constexpr int32 name_internal_to_external(const int32 x) {
        return x - 1;
    }

    constexpr int32 name_external_to_internal(const int32 x) {
        return x + 1;
    }

    struct NameHashEntry {
        uint32 id = 0;
        usize hash = 0;

        constexpr NameHashEntry() = default;

        constexpr NameHashEntry(const uint32 id, const usize hash) : id(id), hash(hash) {}

        [[nodiscard]] constexpr friend bool operator==(const NameHashEntry& a, const NameHashEntry& b) = default;
    };
}

template <>
struct std::hash<retro::core::NameHashEntry> {
    hash() = default;

    [[nodiscard]] constexpr size_t operator()(const retro::core::NameHashEntry& name) const noexcept {
        return retro::core::hash_combine(name.id, name.hash);
    }
};

namespace retro::core {

    struct NameIndices {
        uint32 comparison_index = 0;
        uint32 display_index = 0;

        constexpr NameIndices() = default;

        constexpr NameIndices(const uint32 comparison_index, const uint32 display_index) : comparison_index(comparison_index), display_index(display_index) {}

        [[nodiscard]] constexpr friend bool operator==(const NameIndices& a, const NameIndices& b) = default;

        [[nodiscard]] constexpr bool is_none() const {
            return comparison_index == 0;
        }

        static constexpr NameIndices none() {
            return {};
        }
    };

    export RETRO_API constexpr std::u16string_view NONE_STRING = u"None";

    class RETRO_API NameTable {
        NameTable() = default;

    public:
        static NameTable& instance();

        [[nodiscard]] NameIndices get_or_add_entry(std::u16string_view value, FindType find_type);

        // NOLINTNEXTLINE
        [[nodiscard]] inline bool is_valid(const uint32 comparison_index, const uint32 display_index) const {
            if (comparison_index == 0 || display_index == 0) return false;

            return comparison_strings_.contains(comparison_index) && display_strings_.contains(display_index);
        }

        // NOLINTNEXTLINE
        [[nodiscard]] inline std::u16string_view get_display_string(const uint32 display_index) const {
            const auto existing = display_strings_.find(display_index);
            return existing != display_strings_.end() ? existing->second : NONE_STRING;
        }

        // NOLINTNEXTLINE
        [[nodiscard]] constexpr bool equals_comparison(const uint32 comparison_index, std::u16string_view span) const {
            if (comparison_index == 0) return is_none_span(span);

            const auto value = comparison_strings_.find(comparison_index);
            return value != comparison_strings_.end() && span_equals_string<false>(span, value->second);
        }

    private:
        template <bool CaseSensitive = false>
        static constexpr usize compute_hash(const std::u16string_view span) {
            usize hash = 0;

            for (auto t : span)
            {
                if constexpr (CaseSensitive) {
                    hash = (hash << 5) + hash ^ t;
                }else {
                    const char16_t c = std::tolower(t);
                    hash = (hash << 5) + hash ^ c;
                }
            }
            return hash;
        }

        template <bool CaseSensitive = false>
        static constexpr bool span_equals_string(const std::u16string_view span, const std::u16string_view value) {
            if (span.size() != value.size()) return false;

            for (usize i = 0; i < span.size(); i++) {
                if constexpr (CaseSensitive) {
                    if (span[i] != value[i]) return false;
                } else {
                    if (std::tolower(span[i]) != std::tolower(value[i])) {
                        return false;
                    }
                }
            }

            return true;
        }

        static constexpr bool is_none_span(const std::u16string_view span) {
            return span.empty() || (
                span.size() == 4 && (span[0] == 'N' || span[0] == 'n')
                  && (span[1] == 'o' || span[1] == 'O')
                  && (span[2] == 'n' || span[2] == 'N')
                  && (span[3] == 'e' || span[3] == 'E')
                );
        }

        static constexpr usize BUCKET_COUNT = 1024;
        static constexpr usize BUCKET_MASK = BUCKET_COUNT - 1;

        mutable std::mutex mutex_{};

        std::array<std::unordered_set<NameHashEntry>, BUCKET_COUNT> comparison_buckets_{};
        std::unordered_map<uint32, std::u16string> comparison_strings_{};

        std::array<std::unordered_set<NameHashEntry>, BUCKET_COUNT> display_buckets_{};
        std::unordered_map<uint32, std::u16string> display_strings_{};

        uint32 next_comparison_id_ = 1;
        uint32 next_display_id_ = 1;
    };

    export constexpr int32 NAME_NO_NUMBER = 0;

    export class RETRO_API Name {
        struct LookupOutput {
            uint32 comparison_index = 0;
            uint32 display_index = 0;
            int32 number = 0;
        };

    public:
        constexpr Name() = default;

        explicit(false) Name(std::u16string_view value, FindType find_type = FindType::Add);

        explicit(false) Name(const std::u16string& value, FindType find_type = FindType::Add);

    private:
        inline explicit(false) Name(const LookupOutput indices) : comparison_index_(indices.comparison_index), number_(indices.number), display_index_(indices.display_index) {}

    public:
        [[nodiscard]] constexpr uint32 comparison_index() const {
            return comparison_index_;
        }

        [[nodiscard]] constexpr uint32 display_index() const {
            return display_index_;
        }

        [[nodiscard]] constexpr int32 number() const {
            return number_;
        }

        constexpr void set_number(const int32 number) {
            number_ = number;
        }

        constexpr static Name none() {
            return {};
        }

        // NOLINTNEXTLINE
        [[nodiscard]] inline bool is_valid() const {
            return NameTable::instance().is_valid(comparison_index_, display_index_);
        }

        [[nodiscard]] constexpr bool is_none() const {
            return comparison_index_ == 0;
        }

        [[nodiscard]] std::u16string to_string() const;

        [[nodiscard]] friend constexpr bool operator==(const Name& lhs, const Name& rhs) {
            return lhs.comparison_index_ == rhs.comparison_index_ && lhs.number_ == rhs.number_;
        }

        [[nodiscard]] friend constexpr std::strong_ordering operator<=>(const Name& lhs, const Name& rhs) {
            if (const auto cmp = lhs.comparison_index_ <=> rhs.comparison_index_; cmp != std::strong_ordering::equal) return cmp;
            return lhs.number_ <=> rhs.number_;
        }

        [[nodiscard]] friend RETRO_API bool operator==(const Name& lhs, std::u16string_view rhs);

    private:
        static LookupOutput lookup_name(std::u16string_view value, FindType find_type);
        static std::pair<int32, usize> parse_number(std::u16string_view name);

        uint32 comparison_index_ = 0;
        int32 number_ = 0;
        uint32 display_index_ = 0;
    };
}

export template <>
struct std::hash<retro::core::Name> {
    hash() = default;

    constexpr size_t operator()(const retro::core::Name& name) const noexcept {
        return retro::core::hash_combine(name.comparison_index(), name.number());
    }
};
