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
            std::shared_lock sources_lock{lookup_mutex_};

            const std::u16string current_locale_copy = current_locale_;

            for (const auto &source : sources_)
            {
                const auto result = source->get_localized_string(text_id, current_locale_copy, fallback_source);
                if (result.has_value())
                {
                    return *result;
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

    void LocalizationManager::reload_strings_for_locale(LocalizedTextSourceCategory category)
    {
        // Snapshot the current locale (avoid holding lock during source load)
        std::u16string locale_to_load;
        {
            std::shared_lock lock{lookup_mutex_};
            locale_to_load = current_locale_;
        }

        // Clear and reload the string cache
        {
            std::unique_lock cache_lock{lookup_mutex_};

            // Clear all cached strings
            string_table_.clear();

            // Load from all sources in priority order
            for (const auto &[text_id, string] :
                 sources_ |
                     std::views::transform([&](auto &source)
                                           { return source->load_localized_strings(category, locale_to_load); }) |
                     std::views::join)
            {
                cache_localized_string(text_id, string, 0);
            }
        }

        // Update revisions and notify
        {
            std::unique_lock rev_lock{revision_mutex_};
            if (++global_revision_ == 0)
            {
                ++global_revision_; // Skip 0
            }
            local_revisions_.clear();
        }

        on_revision_changed_();
    }

    void LocalizationManager::cache_localized_string(const TextId text_id,
                                                     LocalizedStringConstPtr string,
                                                     const std::size_t source_hash)
    {
        if (string == nullptr)
        {
            return;
        }

        // Assumes caller holds lookup_mutex_ write lock
        string_table_.emplace(text_id, LocalizedStringEntry{std::move(string), source_hash});

        // Mark this text ID as having a valid local revision
        {
            std::unique_lock rev_lock{revision_mutex_};
            if (auto &local_rev = local_revisions_[text_id]; ++local_rev == 0)
            {
                ++local_rev; // Skip 0
            }
        }
    }
} // namespace retro
