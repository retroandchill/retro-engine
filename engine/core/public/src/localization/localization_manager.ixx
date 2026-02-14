/**
 * @file localization_manager.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization:localization_manager;

import std;
import retro.core.util.enum_class_flags;
import :text_source_types;
import :text_key;
import :localized_text_source;
import retro.core.containers.optional;
import retro.core.util.noncopyable;

namespace retro
{
    export enum class LocalizationManagerInitializedFlags : std::uint8_t
    {
        none = 0,
        engine = 1 << 0,
        game = 1 << 1
    };

    export template <>
    constexpr bool is_flag_enum<LocalizationManagerInitializedFlags> = true;

    export class RETRO_API LocalizationManager
    {
        friend RETRO_API void register_localized_text_source(std::shared_ptr<LocalizedTextSource> source);
        friend RETRO_API TextConstDisplayStringPtr query_localized_text(LocalizedTextSourceCategory category,
                                                                        std::u16string_view source_string);

        LocalizationManager() = default;
        ~LocalizationManager() = default;

        struct SourceEntry
        {
            std::shared_ptr<LocalizedTextSource> source;
            std::int32_t priority;
        };

        mutable std::shared_mutex sources_mutex_;
        std::vector<SourceEntry> text_sources_;

        struct StringHash
        {
            using is_transparent = void; // Enables transparent lookup
            constexpr std::size_t operator()(const std::u16string &v) const
            {
                return std::hash<std::u16string>{}(v);
            }
            constexpr std::size_t operator()(const std::u16string_view v) const
            {
                return std::hash<std::u16string_view>{}(v);
            }
        };
        struct StringEqual
        {
            using is_transparent = void;
            constexpr bool operator()(const std::u16string &lhs, const std::u16string &rhs) const
            {
                return lhs == rhs;
            }
            constexpr bool operator()(const std::u16string &lhs, const std::u16string_view rhs) const
            {
                return lhs == rhs;
            }
            constexpr bool operator()(const std::u16string_view lhs, const std::u16string &rhs) const
            {
                return lhs == rhs;
            }
        };

        mutable std::shared_mutex cache_mutex_;
        std::unordered_map<std::u16string, TextConstDisplayStringPtr, StringHash, StringEqual> display_string_cache_;

      public:
        LocalizationManager(const LocalizationManager &) = delete;
        LocalizationManager(LocalizationManager &&) = delete;
        LocalizationManager &operator=(const LocalizationManager &) = delete;
        LocalizationManager &operator=(LocalizationManager &&) = delete;

        static LocalizationManager &get();

        void register_text_source(std::shared_ptr<LocalizedTextSource> source);

        TextConstDisplayStringPtr find_localized_string(LocalizedTextSourceCategory category,
                                                        std::u16string_view source_string);

        Optional<std::u16string> get_native_culture_name(LocalizedTextSourceCategory category) const;

        std::unordered_set<std::u16string> get_available_cultures(LocalizedTextSourceCategory category) const;

        void clear_cache();
    };

    export RETRO_API void register_localized_text_source(std::shared_ptr<LocalizedTextSource> source);

    export RETRO_API TextConstDisplayStringPtr query_localized_text(LocalizedTextSourceCategory category,
                                                                    std::u16string_view source_string);

    export RETRO_API Optional<std::u16string> get_native_culture_name(LocalizedTextSourceCategory category);

    export RETRO_API std::unordered_set<std::u16string> get_available_cultures(LocalizedTextSourceCategory category);
} // namespace retro
