/**
 * @file registration.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
import retro.core;
import retro.core.interop;

namespace retro
{
    struct RetroEngine_Binds_NameExporter
    {
        static const ExportedFunction RETRO_BIND_Lookup;
        static const ExportedFunction RETRO_BIND_IsValid;
        static const ExportedFunction RETRO_BIND_Compare;
        static const ExportedFunction RETRO_BIND_CompareLexical;
        static const ExportedFunction RETRO_BIND_ToString;
    };

    const ExportedFunction RetroEngine_Binds_NameExporter::RETRO_BIND_Lookup =
        ExportedFunction{"NameExporter", "Lookup", &name_exporter::lookup};
    const ExportedFunction RetroEngine_Binds_NameExporter::RETRO_BIND_IsValid =
        ExportedFunction{"NameExporter", "IsValid", &name_exporter::is_valid};
    const ExportedFunction RetroEngine_Binds_NameExporter::RETRO_BIND_Compare =
        ExportedFunction{"NameExporter", "Compare", &name_exporter::compare};
    const ExportedFunction RetroEngine_Binds_NameExporter::RETRO_BIND_CompareLexical =
        ExportedFunction{"NameExporter", "CompareLexical", &name_exporter::compare_lexical};
    const ExportedFunction RetroEngine_Binds_NameExporter::RETRO_BIND_ToString =
        ExportedFunction{"NameExporter", "ToString", &name_exporter::to_string};

} // namespace retro
