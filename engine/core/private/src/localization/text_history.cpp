/**
 * @file text_history.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

module retro.core.localization.text;

import :text_history;
import retro.core.localization.localization_manager;
import retro.core.localization.text_transformer;

namespace retro
{
    TextRevision TextHistory::revision() const noexcept
    {
        std::shared_lock lock{mutex_};
        return revision_;
    }

    void TextHistory::update_display_string_if_out_of_date()
    {
        if (!can_update_display_string())
            return;

        const auto current_revision = LocalizationManager::get().get_text_revision(text_id());

        std::unique_lock lock{mutex_};

        if (revision_ == current_revision)
            return;

        revision_ = current_revision;
        update_display_string();
    }

    void TextHistory::mark_display_string_out_of_date()
    {
        std::unique_lock lock{mutex_};
        revision_ = TextRevision{0, 0};
    }

    void TextHistory::mark_display_string_up_to_date()
    {
        const bool can_update = can_update_display_string();

        std::unique_lock lock{mutex_};
        revision_ = can_update ? LocalizationManager::get().get_text_revision(text_id()) : TextRevision{0, 0};
    }

    TextHistoryBase::TextHistoryBase(const TextId text_id, std::u16string &&source)
        : text_id_{text_id}, source_{std::move(source)}
    {
    }

    TextHistoryBase::TextHistoryBase(const TextId text_id,
                                     std::u16string &&source,
                                     TextDisplayStringConstPtr &&localized)
        : text_id_{text_id}, source_{std::move(source)}, localized_{std::move(localized)}
    {
    }

    TextId TextHistoryBase::text_id() const noexcept
    {
        return TextId{};
    }

    const std::u16string &TextHistoryBase::source_string() const noexcept
    {
        return source_;
    }

    const std::u16string &TextHistoryBase::display_string() const noexcept
    {
        return localized_ != nullptr ? *localized_ : source_;
    }

    TextDisplayStringConstPtr TextHistoryBase::localized_string() const noexcept
    {
        return localized_;
    }

    std::u16string TextHistoryBase::build_invariant_display_string() const
    {
        return source_;
    }

    bool TextHistoryBase::can_update_display_string() const noexcept
    {
        return !text_id_.is_empty();
    }

    void TextHistoryBase::update_display_string()
    {
        assert(!text_id_.is_empty());

        localized_ = LocalizationManager::get().get_display_string(text_id_.text_namespace(), text_id_.key(), source_);
    }

    TextHistoryGenerated::TextHistoryGenerated(std::u16string &&display_string)
        : display_string_{std::move(display_string)}
    {
    }

    const std::u16string &TextHistoryGenerated::display_string() const noexcept
    {
        return display_string_;
    }

    void TextHistoryGenerated::update_display_string()
    {
        display_string_ = build_localized_display_string();
    }

    TextHistoryTransformed::TextHistoryTransformed(std::u16string &&display_string,
                                                   Text source_text,
                                                   TransformType type)
        : TextHistoryGenerated{std::move(display_string)}, source_text_{std::move(source_text)}, type_{type}
    {
    }

    std::u16string TextHistoryTransformed::build_invariant_display_string() const
    {
        source_text_.rebuild();

        switch (type_)
        {
            case TransformType::to_lower:
                return TextTransformer::to_lower(source_text_.to_string());
            case TransformType::to_upper:
                return TextTransformer::to_upper(source_text_.to_string());
        }

        return u"";
    }

    std::u16string TextHistoryTransformed::build_localized_display_string() const
    {
        return u"";
    }
} // namespace retro
