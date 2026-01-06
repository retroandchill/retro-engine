# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_cityhash STATIC)

target_sources(retro_cityhash PUBLIC
        FILE_SET CXX_MODULES FILES
        cityhash.ixx)


find_package(cityhash CONFIG REQUIRED)
target_link_libraries(retro_cityhash PUBLIC cityhash)

set_target_properties(retro_cityhash PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
