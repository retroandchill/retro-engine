# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_PLATFORM_SOURCES
        private/backend.cpp
        private/platform.cpp
        private/sdl/window.cpp
        private/sdl/backend.cpp
        private/headless/window.cpp
        private/headless/backend.cpp
)

set(RETRO_PLATFORM_HEADERS
)

set (RETRO_PLATFORM_PRIVATE_MODULES
        private/sdl/window.ixx
        private/sdl/backend.ixx
        private/headless/window.ixx
        private/headless/backend.ixx)

set(RETRO_PLATFORM_PUBLIC_MODULES
        public/modules/window.ixx
        public/modules/event.ixx
        public/modules/backend.ixx
)

add_library(retro_platform
        ${RETRO_PLATFORM_SOURCES}
        ${RETRO_PLATFORM_PUBLIC_MODULES}
        ${RETRO_PLATFORM_PRIVATE_MODULES}
        ${RETRO_PLATFORM_HEADERS})

retro_add_boilerplate(retro_platform)

target_include_directories(retro_platform PUBLIC public/include)

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

target_link_libraries(retro_platform PUBLIC retro_core SDL3::Cpp)

set_target_properties(retro_platform PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
