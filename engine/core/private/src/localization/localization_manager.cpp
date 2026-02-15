/**
 * @file localization_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.localization_manager;

namespace retro
{
    LocalizationManager::LocalizationManager() : current_locale_{u"en"}
    {
    }

    LocalizationManager &LocalizationManager::get()
    {
        static LocalizationManager instance;
        return instance;
    }

    LocalizedStringConstPtr LocalizationManager::get_localized_string(TextKey namespace_key,
                                                                      TextKey string_key,
                                                                      std::u16string_view fallback_source) const
    {
        if (string_key.is_empty())
        {
            if (!fallback_source.empty())
            {
                return make_unlocalized_string(std::u16string{fallback_source});
            }
            return nullptr;
        }

        TextId text_id{namespace_key, string_key};

        {
            std::shared_lock lock{lookup_mutex_};

            const auto found_entry = string_table_.find(text_id);
            if (found_entry != string_table_.end())
            {
                // Validate source string match if provided
                if (fallback_source.empty() ||
                    found_entry->second.source_hash == std::hash<std::u16string_view>{}(fallback_source))
                {
                    return found_entry->second.string;
                }
            }
        }

        // Not found or source mismatch - return unlocalized fallback
        if (!fallback_source.empty())
        {
            return make_unlocalized_string(std::u16string{fallback_source});
        }

        return nullptr;
    }

    std::uint16_t LocalizationManager::global_revision() const
    {
        std::shared_lock lock{revision_mutex_};
        return global_revision_;
    }

    TextRevision LocalizationManager::get_text_revision(TextId text_id) const
    {
        if (text_id.is_empty())
        {
            return TextRevision{};
        }

        std::shared_lock lock{revision_mutex_};

        const auto found_local = local_revisions_.find(text_id);
        const std::uint16_t local = (found_local != local_revisions_.end()) ? found_local->second : 0;

        return TextRevision{global_revision_, local};
    }

    void LocalizationManager::register_source(std::shared_ptr<LocalizedTextSource> source)
    {
        if (!source)
        {
            return;
        }

        {
            std::unique_lock lock{lookup_mutex_};
            sources_.push_back(std::move(source));
        }

        // Sort by priority (highest first)
        std::sort(sources_.begin(),
                  sources_.end(),
                  [](const auto &a, const auto &b) { return a->priority() > b->priority(); });
    }

    std::u16string LocalizationManager::current_locale() const
    {
        std::shared_lock lock{lookup_mutex_};
        return current_locale_;
    }

    void LocalizationManager::set_locale(std::u16string_view locale_name)
    {
        {
            std::unique_lock lock{lookup_mutex_};
            current_locale_ = std::u16string{locale_name};
        }

        // Increment global revision to signal all text needs update
        {
            std::unique_lock lock{revision_mutex_};
            if (++global_revision_ == 0)
            {
                ++global_revision_; // Skip 0
            }
            local_revisions_.clear();
        }

        on_revision_changed_();
    }
} // namespace retro
