/**
 * @file localization_manager.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.localization_manager;

namespace retro
{
    LocalizationManager::LocalizationManager() : current_locale_{Locale::create("en_US")}
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

            const auto current_locale_copy = current_locale_;

            for (const auto &source : sources_)
            {
                if (const auto result = source->get_localized_string(text_id, current_locale_copy, fallback_source);
                    result.has_value())
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
        if (source == nullptr)
        {
            return;
        }

        {
            std::unique_lock lock{lookup_mutex_};
            sources_.push_back(std::move(source));
            std::ranges::sort(sources_, [](const auto &a, const auto &b) { return a->priority() > b->priority(); });
        }
    }

    LocalePtr LocalizationManager::current_locale() const
    {
        std::shared_lock lock{lookup_mutex_};
        return current_locale_;
    }

    void LocalizationManager::set_locale(LocalePtr culture_info)
    {
        {
            std::unique_lock lock{lookup_mutex_};
            if (*current_locale_ == *culture_info)
            {
                return;
            }
            current_locale_ = std::move(culture_info);
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
