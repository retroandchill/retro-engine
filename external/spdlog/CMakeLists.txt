# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_spdlog STATIC)

target_sources(retro_spdlog PUBLIC
        FILE_SET CXX_MODULES FILES
        spdlog.ixx)


find_package(spdlog REQUIRED)
target_link_libraries(retro_spdlog PUBLIC spdlog::spdlog)

set_target_properties(retro_spdlog PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
