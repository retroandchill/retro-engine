export module retro.logging.interop:log_exporter;

import retro.core;
import retro.logging;

namespace retro::log_exporter
{
    export void log(LogLevel level, const char16_t *message, int32 length);
}
