# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_SCRIPTING_SOURCES src/dotnet/dotnet_loader.cpp
        src/dotnet/dotnet_manager.cpp)

set(RETRO_SCRIPTING_MODULES src/index.ixx
        src/dotnet/dotnet_loader.ixx
        src/dotnet/index.ixx
        src/dotnet/dotnet_manager.ixx)

add_library(retro_scripting
        ${RETRO_SCRIPTING_SOURCES}
        ${RETRO_SCRIPTING_MODULES}
)

retro_add_boilerplate(retro_scripting)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_scripting PUBLIC RETRO_STATIC)
endif()

target_sources(retro_scripting PUBLIC
        FILE_SET CXX_MODULES FILES
        ${RETRO_SCRIPTING_MODULES}
        )

find_package(unofficial-nethost CONFIG REQUIRED)
target_link_libraries(retro_scripting PUBLIC
        retro_core retro_runtime
        unofficial::nethost::nethost)

add_dependencies(retro_scripting retro_managed)

set_target_properties(retro_scripting PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
