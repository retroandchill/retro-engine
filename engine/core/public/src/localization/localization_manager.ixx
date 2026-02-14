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
import retro.core.util.lazy_singleton;
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
        static constexpr std::int32_t invalid_localization_target_path_id = -1;

        friend RETRO_API void begin_pre_init_text_localization();
        friend RETRO_API void begin_init_text_localization();
        friend RETRO_API void init_engine_text_localization();
        friend RETRO_API void init_game_text_localization();

        struct DisplayStringEntry
        {
            TextConstDisplayStringPtr display_string;
#if RETRO_WITH_EDITOR_DATA
            TextKey loc_res_id;
#endif
            std::int32_t localization_target_path_id = invalid_localization_target_path_id;
            std::uint32_t source_string_hash;

            DisplayStringEntry(TextKey loc_res_id,
                               const std::int32_t localization_target_path_id,
                               const std::uint32_t source_string_hash,
                               TextConstDisplayStringPtr display_string) noexcept
                : display_string{std::move(display_string)},
#if RETRO_WITH_EDITOR_DATA
                  loc_res_id{std::move(loc_res_id)},
#endif
                  localization_target_path_id{localization_target_path_id}, source_string_hash{source_string_hash}
            {
            }
        };

        using DisplayStringLookupTable = std::unordered_map<TextId, DisplayStringEntry>;

        struct DisplayStringsForLocalizationTarget
        {
            std::u16string localization_target_path;
            std::unordered_set<TextId> text_ids;
            bool is_mounted = false;
        };

        struct DisplayStringsByLocalizationTarget
        {
            std::pair<DisplayStringsForLocalizationTarget &, std::int32_t> find_or_add(
                std::u16string_view localization_target_path);
            Optional<DisplayStringsByLocalizationTarget &> find(std::int32_t localization_target_path_id);
            void track_text_id(std::int32_t current_localization_path_id,
                               std::int32_t new_localization_path_id,
                               TextId text_id);

          private:
            std::vector<DisplayStringsForLocalizationTarget> display_strings_;
            std::unordered_map<std::u16string_view, std::int32_t> localization_target_paths_to_ids_;
        };

        std::atomic<LocalizationManagerInitializedFlags> initialized_flags_{LocalizationManagerInitializedFlags::none};

        bool is_initialized() const
        {
            return initialized_flags_ != LocalizationManagerInitializedFlags::none;
        }

        mutable std::shared_mutex display_string_table_mutex_;
        DisplayStringLookupTable display_string_table_;
        DisplayStringsByLocalizationTarget display_strings_by_localization_target_id_;

        mutable std::shared_mutex text_revision_mutex_;
        std::unordered_map<TextId, std::uint16_t> text_revisions_;
        std::uint16_t text_revision_counter_ = 0;

#if RETRO_WITH_EDITOR_DATA
        std::uint8_t game_localization_preview_auto_enable_count_;
        bool is_game_localization_preview_enabled_;
        bool is_localization_locked_;
#endif

        LocalizationManager();
        friend class LazySingleton<LocalizationManager>;

      public:
        ~LocalizationManager() = default;
        LocalizationManager(const LocalizationManager &) = delete;
        LocalizationManager(LocalizationManager &&) = delete;
        LocalizationManager &operator=(const LocalizationManager &) = delete;
        LocalizationManager &operator=(LocalizationManager &&) = delete;

        static LocalizationManager &get();
        static void tear_down();

        static bool is_display_string_support_enabled();

        void compact_data_structures();

        std::u16string requested_language_name() const;
        std::u16string requested_locale_name() const;
        std::u16string get_native_culture_name(LocalizedTextSourceCategory category) const;

        std::vector<std::u16string> get_localized_culture_names(LocalizedTextSourceCategory category) const;

        std::int32_t get_localization_target_path_id(std::u16string_view localization_target_path);

        void register_text_source(std::shared_ptr<LocalizedTextSource> localized_text_source,
                                  bool refresh_resources = true);

        TextConstDisplayStringPtr find_display_string(TextKey ns,
                                                      TextKey key,
                                                      std::u16string_view source_string = {}) const;

        TextConstDisplayStringPtr get_display_string(TextKey ns, TextKey key, std::u16string_view source_string) const;

        std::pair<std::uint16_t, std::uint16_t> get_text_revisions(TextId text_id) const;
    };
} // namespace retro
