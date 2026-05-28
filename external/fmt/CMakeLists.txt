# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_fmt STATIC)

target_sources(retro_fmt PUBLIC
        FILE_SET CXX_MODULES FILES
        fmt.ixx)


find_package(fmt CONFIG REQUIRED)
target_link_libraries(retro_fmt PUBLIC fmt::fmt)

set_target_properties(retro_fmt PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
