# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(spdlog_cppm STATIC)

target_sources(spdlog_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        spdlog.ixx)


find_package(spdlog REQUIRED)
target_link_libraries(spdlog_cppm PUBLIC spdlog::spdlog)

add_library(spdlog::cppm ALIAS spdlog_cppm)

set_target_properties(spdlog_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
