# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_PLATFORM_SOURCES
        src/backend.cpp
)

set(RETRO_PLATFORM_HEADERS )

set (RETRO_PLATFORM_PRIVATE_MODULES )

set(RETRO_PLATFORM_MODULES
        src/dll.ixx
        src/window.ixx
        src/exceptions.ixx
        src/event.ixx
        src/backend.ixx
        src/backend/sdl.ixx
)



add_library(retro_platform ${RETRO_PLATFORM_SOURCES} ${RETRO_PLATFORM_MODULES} ${RETRO_PLATFORM_HEADERS})

retro_add_boilerplate(retro_platform)

target_sources(retro_platform PUBLIC
        FILE_SET CXX_MODULES FILES
        ${RETRO_PLATFORM_MODULES})

find_package(SDL3 REQUIRED)
find_package(Boost REQUIRED COMPONENTS dll)
target_link_libraries(retro_platform PUBLIC retro_core Boost::dll SDL3::SDL3)

set_target_properties(retro_platform PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
