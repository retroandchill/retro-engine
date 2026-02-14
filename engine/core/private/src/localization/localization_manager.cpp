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
    LocalizationManager &LocalizationManager::get()
    {
        static LocalizationManager instance;
        return instance;
    }

    void LocalizationManager::register_text_source(std::shared_ptr<LocalizedTextSource> source)
    {
        if (source == nullptr)
            return;

        std::unique_lock lock{sources_mutex_};
        text_sources_.emplace_back(std::move(source), source->priority());
        std::ranges::sort(text_sources_,
                          [](const SourceEntry &a, const SourceEntry &b) { return a.priority > b.priority; });

        // TODO: Do we actually need to clear the cache?
        std::unique_lock cache_lock{cache_mutex_};
        display_string_cache_.clear();
    }

    TextConstDisplayStringPtr LocalizationManager::find_localized_string(LocalizedTextSourceCategory category,
                                                                         std::u16string_view source_string)
    {
        if (source_string.empty())
            return nullptr;

        {
            std::shared_lock lock{cache_mutex_};
            auto id = display_string_cache_.find(source_string);
            if (id != display_string_cache_.end())
            {
                return id->second;
            }
        }

        {
            std::shared_lock lock{sources_mutex_};
            for (const auto &entry : text_sources_)
            {
                if (auto localized = entry.source->query_localized_string(category, source_string);
                    localized.has_value())
                {
                    auto display_string = make_text_display_string(*localized);
                    {
                        std::unique_lock cache_lock{cache_mutex_};
                        display_string_cache_.emplace(source_string, display_string);
                    }
                    return display_string;
                }
            }
        }

        return nullptr;
    }

    Optional<std::u16string> LocalizationManager::get_native_culture_name(
        const LocalizedTextSourceCategory category) const
    {
        std::shared_lock lock{sources_mutex_};
        for (const auto &entry : text_sources_)
        {
            if (auto culture = entry.source->get_native_culture_name(category))
                return culture;
        }
        return {};
    }
} // namespace retro
