# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(STB_CPP_MODULES image.ixx)

add_library(stb_cpp STATIC
        ${STB_CPP_MODULES})

target_sources(stb_cpp PUBLIC
        FILE_SET CXX_MODULES FILES
        ${STB_CPP_MODULES})


find_package(Stb REQUIRED)
target_include_directories(stb_cpp PRIVATE ${Stb_INCLUDE_DIR})

add_library(Stb::Cpp ALIAS stb_cpp)

set_target_properties(stb_cpp PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
