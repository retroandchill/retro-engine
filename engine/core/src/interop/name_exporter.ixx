export module retro.core.interop:name_exporter;

import retro.core;

namespace retro::name_exporter
{
    export Name lookup(const char16_t *name, int32 length, FindType findType);
    export bool is_valid(Name name);
    export int32 compare(Name lhs, const char16_t *rhs, int32 length);
    export int32 compare_lexical(NameEntryId lhs, NameEntryId rhs, NameCase nameCase);
    export int32 to_string(Name name, char16_t *buffer, int32 bufferSize);
} // namespace retro::name_exporter
