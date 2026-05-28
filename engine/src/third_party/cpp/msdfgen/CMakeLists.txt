# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(msdfgen_cppm OBJECT)

target_sources(msdfgen_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        msdfgen.ixx)


find_package(Freetype REQUIRED)
find_package(msdfgen REQUIRED)
target_link_libraries(msdfgen_cppm PUBLIC msdfgen::msdfgen
        msdfgen::msdfgen-ext)

add_library(msdfgen::cppm ALIAS msdfgen_cppm)

set_target_properties(msdfgen_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
