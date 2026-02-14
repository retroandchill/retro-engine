/**
 * @file loc_key.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization:loc_key;

import std;
import retro.core.algorithm.hashing;
import retro.core.strings.encoding;

namespace retro
{
    export class LocKey
    {
      public:
        constexpr LocKey() = default;

        template <typename Str>
            requires std::constructible_from<std::u16string, Str>
        constexpr explicit(std::convertible_to<Str, std::u16string>) LocKey(Str &&str)
            : str_{std::forward<Str>(str)}, hash_{std::hash<std::u16string>{}(str_)}
        {
        }

        template <typename... Args>
            requires std::constructible_from<std::u16string, Args...>
        constexpr explicit LocKey(std::in_place_t, Args &&...args)
            : str_{std::format(std::forward<Args>(args)...)}, hash_{std::hash<std::u16string>{}(str_)}
        {
        }

        constexpr LocKey(const LocKey &) = default;
        constexpr LocKey(LocKey &&other) noexcept : str_{std::move(other.str_)}, hash_{other.hash_}
        {
            other.hash_ = 0;
        }

        constexpr ~LocKey() = default;

        constexpr LocKey &operator=(const LocKey &) = default;
        constexpr LocKey &operator=(LocKey &&other) noexcept
        {
            if (this == std::addressof(other))
                return *this;

            str_ = std::move(other.str_);
            hash_ = other.hash_;
            other.hash_ = 0;
            return *this;
        }

        constexpr friend bool operator==(const LocKey &lhs, const LocKey &rhs) noexcept
        {
            return lhs.hash_ == rhs.hash_ && lhs.str_ == rhs.str_;
        }

        constexpr friend std::strong_ordering operator<=>(const LocKey &lhs, const LocKey &rhs) noexcept = default;

        [[nodiscard]] constexpr bool empty() const noexcept
        {
            return str_.empty();
        }

        [[nodiscard]] constexpr const std::u16string &str() const noexcept
        {
            return str_;
        }

        static constexpr std::uint32_t produce_hash(std::u16string_view str, const std::uint32_t base_hash = 0)
        {
            return crc32(str, base_hash);
        }

      private:
        friend class std::hash<LocKey>;

        std::u16string str_;
        std::size_t hash_ = 0;
    };

    export struct LocKeyMapLess
    {
        constexpr bool operator()(const std::u16string_view lhs, const std::u16string_view rhs) const noexcept
        {
            return compare<StringComparison::case_sensitive>(lhs, rhs) == std::strong_ordering::less;
        }
    };
} // namespace retro

template <>
struct std::hash<retro::LocKey>
{
    constexpr std::size_t operator()(const retro::LocKey &key) const noexcept
    {
        return key.hash_;
    }
};
