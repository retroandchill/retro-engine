# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_PLATFORM_SOURCES
        private/src/backend.cpp
        private/src/window.cpp
)

set(RETRO_PLATFORM_HEADERS )

set (RETRO_PLATFORM_PRIVATE_MODULES private/src/backend/sdl.ixx)

set(RETRO_PLATFORM_PUBLIC_MODULES
        public/src/dll.ixx
        public/src/window.ixx
        public/src/exceptions.ixx
        public/src/event.ixx
        public/src/backend.ixx
)



add_library(retro_platform ${RETRO_PLATFORM_SOURCES} ${RETRO_PLATFORM_PUBLIC_MODULES} ${RETRO_PLATFORM_PRIVATE_MODULES} ${RETRO_PLATFORM_HEADERS})

retro_add_boilerplate(retro_platform)

target_sources(retro_platform PUBLIC
        FILE_SET retro_platform_public
        TYPE CXX_MODULES
        FILES
        ${RETRO_PLATFORM_PUBLIC_MODULES})

target_sources(retro_platform PRIVATE
        FILE_SET retro_platform_private
        TYPE CXX_MODULES
        FILES
        ${RETRO_PLATFORM_PRIVATE_MODULES})

find_package(SDL3 REQUIRED)
find_package(Boost REQUIRED COMPONENTS dll)
target_link_libraries(retro_platform PUBLIC retro_core Boost::dll SDL3::SDL3)

set_target_properties(retro_platform PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
