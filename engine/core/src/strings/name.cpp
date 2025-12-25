//
// Created by fcors on 12/24/2025.
//
module retro.core.strings;

namespace retro::core {
    NameTable & NameTable::instance() {
        static NameTable table{};
        return table;
    }

    // NOLINTNEXTLINE
    NameIndices NameTable::get_or_add_entry(const std::u16string_view value, const FindType find_type) {
        if (value.empty())
            return NameIndices::none();

        std::scoped_lock lock(mutex_);
        usize hash_ignore = compute_hash(value);
        usize hash_case = compute_hash<true>(value);

        const usize comparison_bucket_index = hash_ignore & BUCKET_MASK;
        const usize display_bucket_index = hash_case & BUCKET_MASK;

        auto &comparison_bucket = comparison_buckets_[comparison_bucket_index];
        auto &display_bucket = display_buckets_[display_bucket_index];

        uint32 comparison_id = 0;
        uint32 display_id = 0;

        for (auto &entry : comparison_bucket) {
            if (entry.hash != hash_ignore) continue;

            if (!span_equals_string<false>(value, comparison_strings_.at(entry.id))) continue;

            comparison_id = entry.id;
            break;
        }

        for (auto &entry : display_bucket) {
            if (entry.hash != hash_case) continue;

            if (!span_equals_string<true>(value, display_strings_.at(entry.id))) continue;

            display_id = entry.id;
            break;
        }

        if (comparison_id != 0 && display_id != 0) return NameIndices{comparison_id, display_id};

        if (find_type == FindType::Find) {
            return NameIndices::none();
        }

        if (comparison_id == 0) {
            comparison_id = next_comparison_id_;
            next_comparison_id_++;
            comparison_bucket.emplace(comparison_id, hash_ignore);
            comparison_strings_.emplace(comparison_id, value);
        }

        if (display_id == 0) {
            display_id = next_display_id_;
            next_display_id_++;
            display_bucket.emplace(display_id, hash_case);
            display_strings_.emplace(display_id, value);
        }

        return NameIndices{comparison_id, display_id};
    }

    Name::Name(const std::u16string_view value, const FindType find_type) : Name(lookup_name(value, find_type)) {
    }

    Name::Name(const std::u16string &value, const FindType find_type) : Name(lookup_name(value, find_type)) {
    }

    // NOLINTNEXTLINE
    Name::LookupOutput Name::lookup_name(const std::u16string_view value, const FindType find_type) {
        if (value.empty())
            return {0, 0, NAME_NO_NUMBER};

        auto [internal_number, new_length] = parse_number(value);
        const auto new_slice = value.substr(0, new_length);

        const auto indices = NameTable::instance().get_or_add_entry(new_slice, find_type);
        return !indices.is_none() ? LookupOutput{indices.comparison_index, indices.display_index, internal_number} : LookupOutput{0, 0, NAME_NO_NUMBER};
    }

    // NOLINTNEXTLINE
    std::pair<int32, usize> Name::parse_number(const std::u16string_view name) {
        usize digits = 0;
        for (isize i = name.size() - 1; i >= 0; i--)
        {
            if (const auto character = name[i]; character < '0' || character > '9')
                break;

            digits++;
        }

        const auto first_digit = name.size() - digits;
        if (first_digit == 0)
            return {NAME_NO_NUMBER, name.size()};

        if (constexpr int MAX_DIGITS = 10;
            digits <= 0
            || digits >= name.size()
            || name[first_digit] != '_'
            || digits > MAX_DIGITS
            || digits != 1 && name[first_digit] == '0'
        )
            return {NAME_NO_NUMBER, name.size()};

        const auto slice = name.substr(first_digit, digits);
        int32 number = 0;
        bool number_is_valid = true;
        for (const auto c : slice) {
            if (c < '0' || c > '9') {
                number_is_valid = false;
                break;
            }

            number = number * 10 + (c - '0');
        }

        return number_is_valid
            ? std::make_pair(number, name.size() - (digits + 1))
            : std::make_pair(NAME_NO_NUMBER, name.size());
    }
}
