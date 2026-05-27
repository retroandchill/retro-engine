# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(SDL3_CPP_MODULES sdl.ixx
        init.ixx
        strings.ixx
        error.ixx
        events.ixx
        utils.ixx
        callback_wrapper.ixx
        video.ixx
        keyboard.ixx
)

add_library(sdl3_cpp OBJECT
    ${SDL3_CPP_MODULES})

target_sources(sdl3_cpp PUBLIC
        FILE_SET CXX_MODULES FILES
        ${SDL3_CPP_MODULES})

find_package(SDL3 REQUIRED)
target_link_libraries(sdl3_cpp PUBLIC SDL3::SDL3)

add_library(SDL3::Cpp ALIAS sdl3_cpp)

set_target_properties(sdl3_cpp PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
