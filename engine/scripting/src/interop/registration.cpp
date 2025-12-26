module retro.scripting;

import retro.scripting;

namespace retro
{
    void register_script_binds()
    {
        using retro::Name;

        const Name name_exporter_name = u"NameExporter";
        BindsManager::register_exported_function(name_exporter_name,
                                                 ExportedFunction(u"Lookup", &name_exporter::lookup));
        BindsManager::register_exported_function(name_exporter_name,
                                                 ExportedFunction(u"IsValid", &name_exporter::is_valid));
        BindsManager::register_exported_function(name_exporter_name,
                                                 ExportedFunction(u"Equals", &name_exporter::equals));
        BindsManager::register_exported_function(name_exporter_name,
                                                 ExportedFunction(u"ToString", &name_exporter::to_string));
    }
} // namespace retro