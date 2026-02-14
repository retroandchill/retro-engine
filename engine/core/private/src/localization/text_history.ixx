/**
 * @file text_history.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization:text_history;

import std;
import :text_data;
import :text_key;
import :text;
import retro.core.util.noncopyable;

namespace retro
{
    enum class TextHistoryType : std::int8_t
    {
        none = -1,
        base = 0,
        named_format,
        ordered_format,
        argument_format,
        as_number,
        as_percent,
        as_currency,
        as_date,
        as_date_time,
        as_transform,
    };

    class TextHistory : public TextData, NonCopyable
    {
      public:
        TextHistory() = default;
        const std::u16string &source_string() const noexcept override
        {
            return display_string();
        }

        TextConstDisplayStringPtr localized_string() const noexcept override
        {
            return nullptr;
        }

        std::uint16_t global_history_revision() const noexcept final;
        std::uint16_t local_history_revision() const noexcept final;

        TextHistory &text_history() final
        {
            return *this;
        }

        const TextHistory &text_history() const final
        {
            return *this;
        }

        virtual TextHistoryType type() const noexcept = 0;

        virtual TextId text_id() const noexcept
        {
            return TextId{};
        }

        virtual std::u16string build_invariant_display_string() const = 0;

        virtual bool identical_to(const TextHistory &other, TextIdenticalModeFlags compare_mode_flags) const = 0;

        static bool static_should_read_from_buffer(std::u16string_view buffer)
        {
            return false;
        }

        virtual bool should_read_from_buffer(const std::u16string_view buffer) const
        {
            return static_should_read_from_buffer(buffer);
        }

        virtual bool read_from_buffer(std::u16string_view buffer,
                                      std::u16string_view text_namespace,
                                      std::u16string_view package_namespace)
        {
            return false;
        }

        virtual bool write_to_buffer(std::u16string &buffer, bool strip_package_namespace) const
        {
            return false;
        }

        virtual std::vector<HistoricFormatData> get_historic_format_data(const Text &text) const
        {
            return {};
        }

        virtual Optional<HistoricTextNumericData> get_historic_numeric_data(const Text &text) const
        {
            return std::nullopt;
        }

        void update_display_string_if_out_of_date();

      protected:
        virtual bool can_update_display_string()
        {
            return true;
        }

        virtual void update_display_string() = 0;

        void mark_display_string_out_of_date();

        void mark_display_string_up_to_date();

      private:
        std::uint16_t global_revision_ = 0;
        std::uint16_t local_revision_ = 0;

        mutable std::mutex mutex_;
    };
} // namespace retro
