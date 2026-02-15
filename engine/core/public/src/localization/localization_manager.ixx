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
import :text_key;
import :localized_string;
import :localized_text_source;
import retro.core.containers.optional;
import retro.core.util.noncopyable;
import retro.core.functional.delegate;

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

    export class RETRO_API LocalizationManager : NonCopyable
    {
        LocalizationManager();
        ~LocalizationManager() = default;

      public:
        static LocalizationManager &get();

        LocalizedStringConstPtr get_localized_string(TextKey namespace_key,
                                                     TextKey string_key,
                                                     std::u16string_view fallback_source = {}) const;

        std::uint16_t global_revision() const;

        TextRevision get_text_revision(TextId text_id) const;

        void register_source(std::shared_ptr<LocalizedTextSource> source);

        std::u16string current_locale() const;
        void set_locale(std::u16string_view locale_name);

        class RevisionChangedEvent : public MulticastDelegate<void()>
        {
            friend LocalizationManager;
        };

        [[nodiscard]] RevisionChangedEvent::RegistrationType on_revision_changed()
        {
            return on_revision_changed_;
        }

      private:
        struct LocalizedStringEntry
        {
            LocalizedStringConstPtr string;
            std::uint32_t source_hash;
        };

        mutable std::shared_mutex lookup_mutex_;
        std::unordered_map<TextId, LocalizedStringEntry> string_table_;

        mutable std::shared_mutex revision_mutex_;
        std::unordered_map<TextId, std::uint16_t> local_revisions_;
        std::uint16_t global_revision_ = 1;

        std::u16string current_locale_;
        std::vector<std::shared_ptr<LocalizedTextSource>> sources_;

        RevisionChangedEvent on_revision_changed_;
    };
} // namespace retro
