//
// Created by fcors on 12/25/2025.
//
module retro.scripting.interop;

namespace retro::scripting {
    void register_all_exports()
    {
        name_exporter::register_exported_functions();
    }
}