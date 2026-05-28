# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_entt STATIC)

target_sources(retro_entt PUBLIC
        FILE_SET CXX_MODULES FILES
        entt.ixx)


find_package(EnTT CONFIG REQUIRED)
target_link_libraries(retro_entt PUBLIC EnTT::EnTT)

set_target_properties(retro_entt PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
