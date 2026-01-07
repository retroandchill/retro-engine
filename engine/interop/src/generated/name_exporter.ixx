/**
 * @file name_exporter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.interop:generated.name_exporter;

import retro.core;
import retro.scripting;

namespace retro::name_exporter
{
    Name lookup(const char16_t *name, int32 length, FindType findType);
    bool is_valid(Name name);
    int32 compare(Name lhs, const char16_t *rhs, int32 length);
    int32 compare_lexical(NameEntryId lhs, NameEntryId rhs, NameCase nameCase);
    int32 to_string(Name name, char16_t *buffer, int32 bufferSize);
} // namespace retro::name_exporter
