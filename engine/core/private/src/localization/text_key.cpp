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
    class TextKeyRegistry
    {
      public:
        [[nodiscard]] static TextKeyRegistry &get()
        {
            static TextKeyRegistry instance;
            return instance;
        }

        [[nodiscard]] TextKey find_or_add(std::u16string_view str)
        {
            if (str.empty())
            {
                return TextKey{0};
            }

            const std::size_t str_hash = std::hash<std::u16string_view>{}(str);

            {
                std::shared_lock lock{mutex_};

                if (const auto found = string_to_id_.find(str_hash); found != string_to_id_.end())
                {
                    return TextKey{found->second};
                }
            }

            {
                std::unique_lock lock{mutex_};

                // Double-check after acquiring write lock
                if (const auto found = string_to_id_.find(str_hash); found != string_to_id_.end())
                {
                    return TextKey{found->second};
                }

                const std::uint32_t new_id = ++next_id_;
                assert(new_id != 0); // Ensure we never use 0 as a valid ID

                string_to_id_.emplace(str_hash, new_id);
                id_to_string_.emplace(new_id, std::u16string{str});

                return TextKey{new_id};
            }
        }

        [[nodiscard]] const std::u16string &get_string(std::uint32_t id)
        {
            std::shared_lock lock{mutex_};

            if (const auto found = id_to_string_.find(id); found != id_to_string_.end())
            {
                return found->second;
            }

            static const std::u16string empty;
            return empty;
        }

      private:
        TextKeyRegistry() = default;

        mutable std::shared_mutex mutex_;
        std::unordered_map<std::size_t, std::uint32_t> string_to_id_;
        std::unordered_map<std::uint32_t, std::u16string> id_to_string_;
        std::uint32_t next_id_ = 0;
    };

    TextKey::TextKey(std::u16string_view key) noexcept : id_{TextKeyRegistry::get().find_or_add(key).id_}
    {
    }

    const std::u16string &TextKey::to_string() const noexcept
    {
        return TextKeyRegistry::get().get_string(id_);
    }
} // namespace retro
