/**
 * @file registration.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
import retro.core;
import retro.logging.interop;

namespace retro
{
    struct RetroEngine_Binds_LogExporter
    {
        static const ExportedFunction RETRO_BIND_Log;
    };

    const ExportedFunction RetroEngine_Binds_LogExporter::RETRO_BIND_Log =
        ExportedFunction{"LogExporter", "Log", &log_exporter::log};
} // namespace retro
