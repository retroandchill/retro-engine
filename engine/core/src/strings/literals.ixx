//
// Created by fcors on 12/23/2025.
//

export module retro.core:literals;

import std;
import :defines;
import :concepts;
import :cstring_view;

namespace retro {
    template<std::size_t N>
    struct FixedString {
        std::array<nchar, N> data;

        // NOLINTNEXTLINE
        consteval FixedString(const char (&str)[N]) noexcept {
            for (usize i = 0; i < N; ++i) {
                data[i] = static_cast<nchar>(str[i]);
            }
        }

        // NOLINTNEXTLINE
        consteval FixedString(const wchar_t (&str)[N]) noexcept {
            static_assert(std::same_as<nchar, wchar_t>, "Cannot use L-literal on non-wide platform");
            for (usize i = 0; i < N; ++i) {
                data[i] = str[i];
            }
        }
    };

    export inline namespace literals {
        template <FixedString Str>
        consteval auto operator""_nc() noexcept {
            return BasicCStringView{Str.data};
        }

        consteval nchar operator""_nc(char c) noexcept {
            return static_cast<nchar>(c);
        }
    }
}