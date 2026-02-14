/**
 * @file text_localization_resource.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.localization:text_localization_resource;

import retro.core.algorithm.hashing;
import std;
import retro.core.io.stream;
import :text_source_types;
import :text_key;

namespace retro
{
    export struct RETRO_API TextLocalizationMetaDataResource
    {
        std::u16string native_culture;

        std::u16string native_locale;

        std::vector<std::u16string> compiled_cultures;

        bool is_ucg = false;

        bool load_from_file(const std::filesystem::path &path);

        StreamResult<void> load_from_stream(Stream &stream, std::u16string_view loc_meta_id);

        bool save_to_file(const std::filesystem::path &path);

        StreamResult<void> save_to_stream(Stream &stream, std::u16string_view loc_meta_id);
    };

    export struct RETRO_API TextLocalizationResource
    {
        static constexpr std::int32_t invalid_localization_target_path_id = -1;

        struct Entry
        {
            TextConstDisplayStringPtr localized_string;
            TextKey loc_res_id;
            std::int32_t localization_target_path_id = invalid_localization_target_path_id;
            std::uint32_t source_string_hash = 0;
            std::int32_t priority = 0;
        };

        using EntriesTable = std::unordered_map<TextKey, Entry>;
        EntriesTable entries;

        static constexpr std::uint32_t hash_string(std::u16string_view str, std::uint32_t base_hash = 0)
        {
            return crc32(str, base_hash);
        }

        void add_entry(TextKey ns,
                       TextKey key,
                       std::u16string_view source_string,
                       std::u16string_view localized_string,
                       std::int32_t priority,
                       TextKey loc_res_id = {});
        void add_entry(TextKey ns,
                       TextKey key,
                       std::u16string_view source_string,
                       const TextConstDisplayStringPtr &localized_string,
                       std::int32_t priority,
                       TextKey loc_res_id = {});
        void add_entry(TextKey ns,
                       TextKey key,
                       std::uint32_t source_hash,
                       std::u16string_view localized_string,
                       std::int32_t priority,
                       TextKey loc_res_id = {});
        void add_entry(TextKey ns,
                       TextKey key,
                       std::uint32_t source_hash,
                       const TextConstDisplayStringPtr &localized_string,
                       std::int32_t priority,
                       TextKey loc_res_id = {});

        bool is_empty() const noexcept;

        void load_from_directory(const std::filesystem::path &directory, std::int32_t priority);

        bool load_from_file(const std::filesystem::path &path, std::int32_t priority);

        StreamResult<void> load_from_stream(Stream &stream, std::u16string_view loc_res_id, std::int32_t priority);
    };
} // namespace retro
