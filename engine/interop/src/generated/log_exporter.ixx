export module retro.interop:generated.log_exporter;

import retro.core;
import retro.logging;
import retro.scripting;

namespace retro::log_exporter
{
    void log(LogLevel level, const char16_t *message, int32 length);
}
