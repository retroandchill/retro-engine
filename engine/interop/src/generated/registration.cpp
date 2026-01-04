/**
 * @file registration.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
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
            ExportedFunction(u"GetEntityTransformOffset", &entity_exporter::get_entity_transform_offset));
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"CreateNewEntity", &entity_exporter::create_new_entity));
        BindsManager::register_exported_function(
            entity_exporter_name,
            ExportedFunction(u"RemoveEntityFromScene", &entity_exporter::remove_entity_from_scene));
    }
} // namespace retro
