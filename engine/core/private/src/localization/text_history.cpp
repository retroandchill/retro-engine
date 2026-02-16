/**
 * @file text_history.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.text_history;

namespace retro
{
    TextRevision TextHistory::revision() const noexcept
    {
        std::shared_lock lock{mutex_};
        return revision_;
    }

    TextHistoryBase::TextHistoryBase(TextId text_id, std::u16string &&source)
        : text_id_{text_id}, source_{std::move(source)}
    {
    }

    TextHistoryBase::TextHistoryBase(TextId text_id, std::u16string &&source, TextDisplayStringConstPtr &&localized)
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
} // namespace retro
