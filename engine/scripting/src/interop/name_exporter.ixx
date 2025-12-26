//
// Created by fcors on 12/25/2025.
//
export module retro.scripting.interop:name_exporter;

import std;
import retro.core;
import retro.core.strings;

using retro::core::Name;
using retro::core::FindType;

namespace retro::scripting::name_exporter {
    void register_exported_functions();

    Name lookup(const char16_t* name, int32 length, FindType find_type);

    bool is_valid(Name name);

    bool equals(Name lhs, const char16_t* rhs, int32 rhs_len);

    int32 to_string(Name name, char16_t* buffer, int32 length);
}