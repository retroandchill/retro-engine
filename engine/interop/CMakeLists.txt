# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_INTEROP_GEN_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/generated)

add_custom_target(retro_generate_interop
        COMMAND "${CMAKE_COMMAND}" -E make_directory "${RETRO_INTEROP_GEN_DIR}"
        COMMAND dotnet "${RETRO_BIN_DIR}/RetroEngine.NativeBindsGenerator.dll"
        --source-assemblies "${RETRO_BIN_DIR}/RetroEngine.Strings.dll"
        --source-assemblies "${RETRO_BIN_DIR}/RetroEngine.dll"
        --output-directory "${RETRO_INTEROP_GEN_DIR}"
        --module-name "retro.interop"
        --fragment-prefix "generated"
        DEPENDS retro_managed
        COMMENT "Regenerating interop C++ module from managed assemblies"
        VERBATIM)

add_library(retro_interop
        src/generated/name_exporter.cpp
        src/generated/registration.cpp
        src/generated/entity_exporter.cpp
)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_interop PUBLIC RETRO_STATIC)
endif()

target_sources(retro_interop PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/generated/registration.ixx
        src/generated/name_exporter.ixx
        src/generated/entity_exporter.ixx
        src/generated/index.ixx)

target_link_libraries(retro_interop PUBLIC
        retro_scripting
        retro_renderer)

add_dependencies(retro_interop retro_managed)

set_target_properties(retro_interop PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
