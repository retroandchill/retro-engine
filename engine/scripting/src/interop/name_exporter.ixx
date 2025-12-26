export module retro.scripting:interop.name_exporter;

import retro.core;    

namespace retro::name_exporter {
    export Name lookup(const char16_t* name, int32 length, FindType findType);
    export bool is_valid(Name name);
    export bool equals(Name lhs, const char16_t* rhs, int32 length);
    export int32 to_string(Name name, char16_t* buffer, int32 bufferSize);
}