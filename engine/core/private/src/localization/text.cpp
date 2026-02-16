/**
 * @file text.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.core.localization.text;

import :text_history;
import retro.core.localization.text_transformer;

namespace retro
{
    const Text &Text::empty()
    {
        static Text empty_text{make_ref_counted<TextHistoryBase>()};
        return empty_text;
    }

    Text::Text() : Text{empty().text_data_}
    {
    }

    Text::Text(std::u16string &&source_string)
        : Text{make_ref_counted<TextHistoryBase>(TextId{}, std::move(source_string)), TextFlag::initialized_from_string}
    {
    }

    Text::Text(Text &&other) noexcept : text_data_(std::move(other.text_data_)), flags_(other.flags_)
    {
        other.text_data_ = empty().text_data_;
        other.flags_ = TextFlag::none;
    }

    Text &Text::operator=(Text &&other) noexcept
    {
        text_data_ = std::move(other.text_data_);
        flags_ = other.flags_;
        other.text_data_ = empty().text_data_;
        other.flags_ = TextFlag::none;
        return *this;
    }

    Text Text::from_name(const Name value)
    {
        return Text{value.to_string<char16_t>()};
    }

    Text Text::as_culture_invariant(std::u16string &&source_string)
    {
        return Text{make_ref_counted<TextHistoryBase>(TextId{}, std::move(source_string)), TextFlag::culture_invariant};
    }

    Text Text::as_culture_invariant(Text text)
    {
        auto new_text = std::move(text);
        new_text.flags_ |= TextFlag::culture_invariant;
        return new_text;
    }

    const std::u16string &Text::to_string() const noexcept
    {
        static std::u16string empty{};
        if (text_data_ == nullptr)
        {
            return empty;
        }

        return text_data_->display_string();
    }

    std::strong_ordering Text::compare_to(const Text &other, TextComparisonLevel) const
    {
        return to_string() <=> other.to_string();
    }

    std::strong_ordering Text::compare_to_ignore_case(const Text &other) const noexcept
    {
        return compare<StringComparison::case_insensitive>(to_string(), other.to_string());
    }

    bool Text::equals(const Text &other, TextComparisonLevel comparison_level) const noexcept
    {
        return compare_to(other, comparison_level) == std::strong_ordering::equal;
    }

    bool Text::equals_ignore_case(const Text &other) const noexcept
    {
        return compare_to_ignore_case(other) == std::strong_ordering::equal;
    }

    bool Text::is_empty() const noexcept
    {
        return to_string().empty();
    }

    bool Text::is_empty_or_whitespace() const noexcept
    {
        return retro::is_empty_or_whitespace(to_string());
    }

    Text Text::to_lower() const
    {
        Text result{make_ref_counted<TextHistoryTransformed>(TextTransformer::to_lower(to_string()),
                                                             *this,
                                                             TextHistoryTransformed::TransformType::to_lower)};
        result.flags_ |= TextFlag::transient;
        return result;
    }

    Text Text::to_upper() const
    {
        Text result{make_ref_counted<TextHistoryTransformed>(TextTransformer::to_upper(to_string()),
                                                             *this,
                                                             TextHistoryTransformed::TransformType::to_upper)};
        result.flags_ |= TextFlag::transient;
        return result;
    }

    bool Text::is_transient() const noexcept
    {
        return has_any_flags(flags_, TextFlag::transient);
    }

    bool Text::is_culture_invariant() const noexcept
    {
        return has_any_flags(flags_, TextFlag::culture_invariant);
    }

    bool Text::is_initialized_from_string() const noexcept
    {
        return has_any_flags(flags_, TextFlag::initialized_from_string);
    }

    void Text::rebuild() const
    {
        text_data_->mutable_text_history().update_display_string_if_out_of_date();
    }
} // namespace retro
