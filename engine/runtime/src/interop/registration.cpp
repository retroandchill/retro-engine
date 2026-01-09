import retro.core;
import retro.runtime.interop;

namespace retro
{
    struct RetroEngine_Binds_EntityExporter
    {
        static const ExportedFunction RETRO_BIND_GetEntityTransform;
        static const ExportedFunction RETRO_BIND_SetEntityTransform;
        static const ExportedFunction RETRO_BIND_CreateNewEntity;
        static const ExportedFunction RETRO_BIND_RemoveEntityFromScene;
    };

    const ExportedFunction RetroEngine_Binds_EntityExporter::RETRO_BIND_GetEntityTransform =
        ExportedFunction{"EntityExporter", "GetEntityTransform", &entity_exporter::get_entity_transform};
    const ExportedFunction RetroEngine_Binds_EntityExporter::RETRO_BIND_SetEntityTransform =
        ExportedFunction{"EntityExporter", "SetEntityTransform", &entity_exporter::set_entity_transform};
    const ExportedFunction RetroEngine_Binds_EntityExporter::RETRO_BIND_CreateNewEntity =
        ExportedFunction{"EntityExporter", "CreateNewEntity", &entity_exporter::create_new_entity};
    const ExportedFunction RetroEngine_Binds_EntityExporter::RETRO_BIND_RemoveEntityFromScene =
        ExportedFunction{"EntityExporter", "RemoveEntityFromScene", &entity_exporter::remove_entity_from_scene};

} // namespace retro
