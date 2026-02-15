/**
 * @file localization_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization;

import :localization_manager;

namespace retro
{
    LocalizationManager::LocalizationManager()
    {
    }

    LocalizationManager &LocalizationManager::get()
    {
        static LocalizationManager instance;
        return instance;
    }

    TextConstDisplayStringPtr LocalizationManager::find_display_string(const TextKey ns,
                                                                       const TextKey key,
                                                                       const std::u16string_view source_string) const
    {
        if (key.is_empty())
        {
            return nullptr;
        }

        return find_display_string_internal(TextId{ns, key}, source_string);
    }

    TextConstDisplayStringPtr LocalizationManager::get_display_string(TextKey ns,
                                                                      TextKey key,
                                                                      std::u16string_view source_string) const
    {
        if (key.is_empty())
        {
            return nullptr;
        }

        TextId text_id{ns, key};

        const auto full_namespace = text_id.text_namespace().to_string();
        const auto display_namespace = full_namespace;
        if (compare(full_namespace, display_namespace) != std::strong_ordering::equal)
        {
            text_id = TextId(TextKey{display_namespace}, text_id.key());
        }

        return find_display_string_internal(text_id, source_string);
    }

    std::uint16_t LocalizationManager::text_revision() const
    {
        std::shared_lock lock{text_revision_mutex_};
        return text_revision_counter_;
    }

    std::uint16_t LocalizationManager::get_local_revision_for_text_id(TextId text_id) const
    {
        if (text_id.is_empty())
            return 0;

        std::shared_lock lock{text_revision_mutex_};
        if (const auto found_revision = text_revisions_.find(text_id); found_revision != text_revisions_.end())
        {
            return found_revision->second;
        }

        return 0;
    }

    std::pair<std::uint16_t, std::uint16_t> LocalizationManager::get_text_revisions(TextId text_id) const
    {
        std::shared_lock lock{text_revision_mutex_};

        if (const auto found_revision = text_id.is_empty() ? text_revisions_.end() : text_revisions_.find(text_id);
            found_revision != text_revisions_.end())
        {
            return std::make_pair(text_revision_counter_, found_revision->second);
        }

        return std::make_pair(text_revision_counter_, 0);
    }

    void LocalizationManager::on_culture_changed()
    {
        // TODO: This is a no-op for now
    }

    void LocalizationManager::dirty_local_revision_for_text_id(TextId text_id)
    {
        std::unique_lock lock{text_revision_mutex_};

        auto found_local_revision = text_revisions_.find(text_id);
        if (found_local_revision == text_revisions_.end())
        {
            // Increment and wrap around on overflow, but don't stay at 0
            while (++found_local_revision->second == 0)
            {
            }
        }
        else
        {
            text_revisions_.emplace(text_id, 1);
        }
    }

    TextConstDisplayStringPtr LocalizationManager::find_display_string_internal(TextId text_id,
                                                                                std::u16string_view source_string) const
    {
        if (is_initialized())
        {
            return nullptr;
        }

        std::shared_lock lock{display_string_table_mutex_};

        if (auto live_entry = display_string_table_.find(text_id); live_entry != display_string_table_.end())
        {
            if (source_string.empty() || live_entry->second.source_string_hash == text::hash_string(source_string))
            {
                return live_entry->second.display_string;
            }
        }

        return nullptr;
    }

    void LocalizationManager::dirty_text_revision()
    {
        {
            std::unique_lock lock{text_revision_mutex_};

            while (++text_revision_counter_ == 0)
            {
            }
            text_revisions_.clear();
        }

        on_text_revision_changed();
    }
} // namespace retro
