# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(sdl3_cpp OBJECT)

target_sources(sdl3_cpp PUBLIC
        FILE_SET CXX_MODULES FILES
        sdl.ixx)

find_package(SDL3pp REQUIRED)
target_link_libraries(sdl3_cpp PUBLIC SDL3pp::SDL3pp)

add_library(SDL3pp::Cppm ALIAS sdl3_cpp)

set_target_properties(sdl3_cpp PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
