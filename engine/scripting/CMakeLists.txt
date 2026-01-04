# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_scripting
        src/dotnet/dotnet_loader.cpp
        src/dotnet/dotnet_manager.cpp
        src/binds/binds_manager.cpp
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
        src/binds/exported_function.ixx)

find_package(unofficial-nethost CONFIG REQUIRED)
target_link_libraries(retro_scripting PUBLIC
        retro_core retro_runtime
        unofficial::nethost::nethost)

add_dependencies(retro_scripting retro_managed)

set_target_properties(retro_scripting PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
