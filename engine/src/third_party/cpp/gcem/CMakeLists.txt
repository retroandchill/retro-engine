# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(gcem_cppm STATIC)

target_sources(gcem_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        gcem.ixx)


find_package(gcem CONFIG REQUIRED)
target_link_libraries(gcem_cppm PUBLIC gcem)

add_library(gcem::cppm ALIAS gcem_cppm)

set_target_properties(gcem_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
