# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_SCRIPTING_SOURCES src/backend/dotnet.cpp
        src/services.cpp)

set(RETRO_SCRIPTING_PUBLIC_MODULES src/services.ixx)

set(RETRO_SCRIPTING_PRIVATE_MODULES src/backend/dotnet.ixx)

add_library(retro_scripting
        ${RETRO_SCRIPTING_SOURCES}
        ${RETRO_SCRIPTING_PUBLIC_MODULES}
        ${RETRO_SCRIPTING_PRIVATE_MODULES}
)

retro_add_boilerplate(retro_scripting)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_scripting PUBLIC RETRO_STATIC)
endif()

target_sources(retro_scripting PUBLIC
        FILE_SET retro_scripting_public
        TYPE CXX_MODULES FILES
        ${RETRO_SCRIPTING_PUBLIC_MODULES}
        )

target_sources(retro_scripting PRIVATE
        FILE_SET retro_scripting_private
        TYPE CXX_MODULES FILES
        ${RETRO_SCRIPTING_PRIVATE_MODULES}
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
