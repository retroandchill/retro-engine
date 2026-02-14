/**
 * @file text_key.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization:text_key;

import std;
import retro.core.type_traits.range;
import retro.core.algorithm.hashing;

namespace retro
{
    export class RETRO_API TextKey
    {
        static constexpr std::int32_t index_none = -1;

      public:
        constexpr TextKey() = default;

        constexpr explicit TextKey(std::u16string_view key) noexcept;

        std::u16string to_string() const;
        void append_string(std::u16string &out) const;

        constexpr friend bool operator==(const TextKey &lhs, const TextKey &rhs) noexcept = default;

        constexpr bool is_empty() const noexcept
        {
            return index_ == index_none;
        }

        constexpr void reset()
        {
            index_ = index_none;
        }

        static void compact_data_structures();

        static void tear_down();

      private:
        std::int32_t index_ = index_none;
    };

    export class TextId
    {
      public:
        constexpr TextId() = default;

        constexpr TextId(const TextKey ns, const TextKey key) noexcept : namespace_{ns}, key_{key}
        {
        }

        [[nodiscard]] TextKey text_namespace() const noexcept
        {
            return namespace_;
        }
        [[nodiscard]] TextKey key() const noexcept
        {
            return key_;
        }

        constexpr friend bool operator==(const TextId &lhs, const TextId &rhs)
        {
            return lhs.namespace_ == rhs.namespace_ && lhs.key_ == rhs.key_;
        }

        constexpr bool is_empty() const noexcept
        {
            return namespace_.is_empty() && key_.is_empty();
        }

        constexpr void reset() noexcept
        {
            namespace_.reset();
            key_.reset();
        }

      private:
        friend struct std::hash<TextId>;

        TextKey namespace_;
        TextKey key_;
    };
} // namespace retro

template <>
struct std::hash<retro::TextKey>
{
    RETRO_API std::size_t operator()(const retro::TextKey &key) const;
};

template <>
struct std::hash<retro::TextId>
{
    std::size_t operator()(const retro::TextId &id) const
    {
        return hash_combine(id.namespace_, id.key_);
    }
};
