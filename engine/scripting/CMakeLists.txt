add_library(retro_scripting
        src/dotnet/dotnet_loader.cpp
        src/dotnet/dotnet_manager.cpp
        src/binds/binds_manager.cpp
        src/interop/name_exporter.cpp
        src/interop/export_registration.cpp
)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_scripting PUBLIC RETRO_STATIC)
endif()

target_sources(retro_scripting PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/dotnet/dotnet_loader.ixx
        src/dotnet/index.ixx
        src/dotnet/dotnet_manager.ixx
        src/binds/binds_manager.ixx
        src/binds/index.ixx
        src/binds/exported_function.ixx
        src/interop/export_registration.ixx
        src/interop/name_exporter.ixx)

find_package(SDL3 CONFIG REQUIRED)
find_package(unofficial-nethost CONFIG REQUIRED)
target_link_libraries(retro_scripting PUBLIC
        retro_core
        retro_platform
        unofficial::nethost::nethost
        SDL3::SDL3)

add_dependencies(retro_scripting retro_managed)

set_target_properties(retro_scripting PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")