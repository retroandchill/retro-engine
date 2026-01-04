module retro.interop;

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

        const Name entity_exporter_name = u"EntityExporter";
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"GetEntityTransform", &entity_exporter::get_entity_transform));
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"SetEntityTransform", &entity_exporter::set_entity_transform));
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"CreateNewEntity", &entity_exporter::create_new_entity));
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"RemoveEntityFromScene", &entity_exporter::remove_entity_from_scene));

        const Name log_exporter_name = u"LogExporter";
        BindsManager::register_exported_function(log_exporter_name, ExportedFunction(u"Log", &log_exporter::log));
    }
} // namespace retro
