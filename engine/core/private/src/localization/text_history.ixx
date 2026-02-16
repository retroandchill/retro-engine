/**
 * @file text_history.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.localization.text_history;

import retro.core.localization.text_data;

import std;
import retro.core.containers.optional;
import retro.core.localization.text_key;

namespace retro
{
    export class TextHistory : public TextData
    {
      public:
        const std::u16string &source_string() const noexcept override
        {
            return display_string();
        }

        TextDisplayStringConstPtr localized_string() const noexcept override
        {
            return nullptr;
        }

        TextRevision revision() const noexcept final;

        TextId text_id() const noexcept override
        {
            return TextId{};
        }

      private:
        mutable std::shared_mutex mutex_;
        TextRevision revision_;
    };

    export class TextHistoryBase : public TextHistory
    {
      public:
        TextHistoryBase(TextId text_id, std::u16string &&source);
        TextHistoryBase(TextId text_id, std::u16string &&source, TextDisplayStringConstPtr &&localized);

        TextId text_id() const noexcept final;
        const std::u16string &source_string() const noexcept override;
        const std::u16string &display_string() const noexcept override;
        TextDisplayStringConstPtr localized_string() const noexcept override;

      private:
        TextId text_id_;
        std::u16string source_;
        TextDisplayStringConstPtr localized_;
    };
} // namespace retro
