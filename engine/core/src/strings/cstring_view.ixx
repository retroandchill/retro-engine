//
// Created by fcors on 12/20/2025.
//
module;

#include <cassert>

export module retro.core.strings:cstring_view;

import std;
import retro.core;

import :concepts;

namespace retro::core {
    /**
     * @brief Encapsulates a view of a C-style string or a standard library string
     * while ensuring it is null-terminated.
     *
     * @tparam T The character type of the string (e.g., char, wchar_t, etc.).
     */
    export template<Char T>
    class BasicCStringView {
        using ViewType = std::basic_string_view<T>;

    public:
        using traits_type = ViewType::traits_type;
        using value_type = ViewType::value_type;
        using pointer = ViewType::pointer;
        using const_pointer = ViewType::const_pointer;
        using reference = ViewType::reference;
        using const_reference = ViewType::const_reference;
        using const_iterator = ViewType::const_iterator;
        using iterator = ViewType::iterator;
        using const_reverse_iterator = ViewType::const_reverse_iterator;
        using reverse_iterator = ViewType::reverse_iterator;
        using size_type = ViewType::size_type;
        using difference_type = ViewType::difference_type;

        static constexpr size_type npos = ViewType::npos;

        template<size_type N>
        constexpr explicit(false) BasicCStringView(const T (&str)[N]) noexcept : view{str, N - 1} {
            assert(str[N - 1] == '\0');
        }

        explicit(false) BasicCStringView(const std::basic_string<T> &str) noexcept : view{str} {
        }

        explicit(false) BasicCStringView(std::basic_string<T> &&str) = delete;

        [[nodiscard]] constexpr const_iterator begin() const noexcept { return view.begin(); }

        [[nodiscard]] constexpr const_iterator cbegin() const noexcept { return view.cbegin(); }

        [[nodiscard]] constexpr const_iterator end() const noexcept { return view.end(); }

        [[nodiscard]] constexpr const_iterator cend() const noexcept { return view.cend(); }

        [[nodiscard]] constexpr const_reverse_iterator rbegin() const noexcept { return view.rbegin(); }

        [[nodiscard]] constexpr const_reverse_iterator crbegin() const noexcept { return view.crbegin(); }

        [[nodiscard]] constexpr const_reverse_iterator rend() const noexcept { return view.rend(); }

        [[nodiscard]] constexpr const_reverse_iterator crend() const noexcept { return view.crend(); }

        [[nodiscard]] constexpr const T &operator[](size_type index) const noexcept {
            return view[index];
        }

        [[nodiscard]] constexpr const T &at(size_type index) const noexcept {
            return view.at(index);
        }

        [[nodiscard]] constexpr const T &front() const noexcept { return view.front(); }

        [[nodiscard]] constexpr const T &back() const noexcept { return view.back(); }


        [[nodiscard]] constexpr const T *data() const noexcept { return view.data(); }

        [[nodiscard]] constexpr size_type size() const noexcept { return view.size(); }

        [[nodiscard]] constexpr size_type length() const noexcept { return view.length(); }

        [[nodiscard]] constexpr size_type max_size() noexcept { return view.max_size(); }

        [[nodiscard]] constexpr bool empty() const noexcept { return view.empty(); }

        [[nodiscard]] constexpr std::basic_string_view<T> to_string_view() const noexcept {
            return view;
        }

        [[nodiscard]] explicit(false) constexpr operator std::basic_string_view<T>() const noexcept {
            return to_string_view();
        }

        [[nodiscard]] constexpr std::basic_string<T> to_string() const noexcept { return std::basic_string<T>{view}; }

        [[nodiscard]] constexpr std::basic_string_view<T> remove_prefix(size_type n) const noexcept {
            auto view_copy = view;
            view_copy.remove_prefix(n);
            return view_copy;
        }

        [[nodiscard]] constexpr std::basic_string_view<T> remove_suffix(size_type n) const noexcept {
            auto view_copy = view;
            view_copy.remove_suffix(n);
            return view_copy;
        }

        [[nodiscard]] constexpr size_type copy(T *dest, size_type n, size_type offset = 0) const {
            return view.copy(dest, n, offset);
        }

        [[nodiscard]] constexpr std::basic_string_view<T> substr(size_type offset = 0, size_type count = npos) const {
            return view.substr(offset, count);
        }

        [[nodiscard]] constexpr bool starts_with(const std::basic_string_view<T> other) const noexcept {
            return view.starts_with(other);
        }

        [[nodiscard]] constexpr bool starts_with(const T other) const noexcept {
            return view.starts_with(other);
        }

        [[nodiscard]] constexpr bool starts_with(const T *other) const noexcept {
            return view.starts_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const std::basic_string_view<T> other) const noexcept {
            return view.ends_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const T other) const noexcept {
            return view.ends_with(other);
        }

        [[nodiscard]] constexpr bool ends_with(const T *other) const noexcept {
            return view.ends_with(other);
        }

        [[nodiscard]] constexpr bool contains(const std::basic_string_view<T> other) const noexcept {
            return view.contains(other);
        }

        [[nodiscard]] constexpr bool contains(const T other) const noexcept {
            return view.contains(other);
        }

        [[nodiscard]] constexpr bool contains(const T *other) const noexcept {
            return view.contains(other);
        }

        [[nodiscard]] constexpr size_type find(const std::basic_string_view<T> other,
                                               size_type pos = 0) const noexcept {
            return view.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T other, size_type pos = 0) const noexcept {
            return view.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T *other, size_type pos = 0) const noexcept {
            return view.find(other, pos);
        }

        [[nodiscard]] constexpr size_type find(const T *other, size_type pos, size_type count) const noexcept {
            return view.find(other, pos, count);
        }

        [[nodiscard]] constexpr size_type rfind(const std::basic_string_view<T> other,
                                                size_type pos = npos) const noexcept {
            return view.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T other, size_type pos = npos) const noexcept {
            return view.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T *other, size_type pos = npos) const noexcept {
            return view.rfind(other, pos);
        }

        [[nodiscard]] constexpr size_type rfind(const T *other, size_type pos, size_type count) const noexcept {
            return view.rfind(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_first_of(const std::basic_string_view<T> other,
                                                        size_type pos = 0) const noexcept {
            return view.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T other, size_type pos = 0) const noexcept {
            return view.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T *other, size_type pos = 0) const noexcept {
            return view.find_first_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_of(const T *other, size_type pos, size_type count) const noexcept {
            return view.find_first_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_last_of(const std::basic_string_view<T> other,
                                                       size_type pos = npos) const noexcept {
            return view.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T other, size_type pos = npos) const noexcept {
            return view.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T *other, size_type pos = npos) const noexcept {
            return view.find_last_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_of(const T *other, size_type pos, size_type count) const noexcept {
            return view.find_last_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const std::basic_string_view<T> other,
                                                            size_type pos = 0) const noexcept {
            return view.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T other, size_type pos = 0) const noexcept {
            return view.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T *other, size_type pos = 0) const noexcept {
            return view.find_first_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_first_not_of(const T *other, size_type pos,
                                                            size_type count) const noexcept {
            return view.find_first_not_of(other, pos, count);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const std::basic_string_view<T> other,
                                                           size_type pos = npos) const noexcept {
            return view.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T other, size_type pos = npos) const noexcept {
            return view.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T *other, size_type pos = npos) const noexcept {
            return view.find_last_not_of(other, pos);
        }

        [[nodiscard]] constexpr size_type find_last_not_of(const T *other, size_type pos,
                                                           size_type count) const noexcept {
            return view.find_last_not_of(other, pos, count);
        }

        [[nodiscard]] friend constexpr bool operator==(const BasicCStringView &a, const BasicCStringView &b) noexcept
        = default;

        [[nodiscard]] friend constexpr bool operator==(const BasicCStringView &self,
                                                       const std::basic_string_view<T> other) noexcept {
            return self == other;
        }

        [[nodiscard]] friend constexpr auto operator<=>(const BasicCStringView &a, const BasicCStringView &b) noexcept {
            return a.view <=> b.view;
        }

        [[nodiscard]] friend constexpr auto operator<=>(const BasicCStringView &self,
                                                        const std::basic_string_view<T> other) noexcept {
            return self.view <=> other;
        }

        friend auto operator<<(std::basic_ostream<T, traits_type> &stream, const BasicCStringView &view) {
            return stream << view.view;
        }

    private:
        friend struct std::hash<BasicCStringView>;

        std::basic_string_view<T> view{};
    };

    export using CStringView = BasicCStringView<char>;
    export using WCStringView = BasicCStringView<wchar_t>;
    export using U8CStringView = BasicCStringView<char8_t>;
    export using U16CStringView = BasicCStringView<char16_t>;
    export using U32CStringView = BasicCStringView<char32_t>;
}

export template<>
struct std::hash<retro::core::CStringView> {
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::core::CStringView &view) const noexcept {
        return hash<string_view>{}(view.view);
    }
};

export template<>
struct std::hash<retro::core::WCStringView> {
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::core::WCStringView &view) const noexcept {
        return hash<wstring_view>{}(view.view);
    }
};

export template<>
struct std::hash<retro::core::U8CStringView> {
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::core::U8CStringView &view) const noexcept {
        return hash<u8string_view>{}(view.view);
    }
};

export template<>
struct std::hash<retro::core::U16CStringView> {
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::core::U16CStringView &view) const noexcept {
        return hash<u16string_view>{}(view.view);
    }
};

export template<>
struct std::hash<retro::core::U32CStringView> {
    hash() = default;

    [[nodiscard]] inline size_t operator()(const retro::core::U32CStringView &view) const noexcept {
        return hash<u32string_view>{}(view.view);
    }
};
