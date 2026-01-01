export module retro.interop:generated.name_exporter;

import retro.core;

namespace retro::name_exporter
{
    Name lookup(const char16_t *name, int32 length, FindType findType);
    bool is_valid(Name name);
    bool equals(Name lhs, const char16_t *rhs, int32 length);
    int32 to_string(Name name, char16_t *buffer, int32 bufferSize);
} // namespace retro::name_exporter