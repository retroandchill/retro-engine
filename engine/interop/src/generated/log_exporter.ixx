/**
 * @file log_exporter.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.interop:generated.log_exporter;

import retro.core;
import retro.logging;
import retro.scripting;

namespace retro::log_exporter
{
    void log(LogLevel level, const char16_t *message, int32 length);
}
