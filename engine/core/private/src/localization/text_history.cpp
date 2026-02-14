/**
 * @file text_history.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization;

import :text_history;
import :localization_manager;

namespace retro
{
    std::uint16_t TextHistory::global_history_revision() const noexcept
    {
        std::scoped_lock lock{mutex_};
        return global_revision_;
    }

    std::uint16_t TextHistory::local_history_revision() const noexcept
    {
        std::scoped_lock lock{mutex_};
        return local_revision_;
    }

    void TextHistory::update_display_string_if_out_of_date()
    {
        if (!can_update_display_string())
            return;

        auto [current_global_revision, current_local_revision] =
            LocalizationManager::get().get_text_revisions(text_id());

        std::scoped_lock lock{mutex_};
        if (global_revision_ != current_global_revision || local_revision_ != current_local_revision)
        {
            global_revision_ = current_global_revision;
            local_revision_ = current_local_revision;

            update_display_string();
        }
    }

    void TextHistory::mark_display_string_out_of_date()
    {
        std::scoped_lock lock{mutex_};
        global_revision_ = 0;
        local_revision_ = 0;
    }

    void TextHistory::mark_display_string_up_to_date()
    {
        const bool can_update = can_update_display_string();

        std::scoped_lock lock{mutex_};

        if (can_update)
        {
            auto [g, l] = LocalizationManager::get().get_text_revisions(text_id());
            global_revision_ = g;
            local_revision_ = l;
        }
        else
        {
            global_revision_ = 0;
            local_revision_ = 0;
        }
    }
} // namespace retro
