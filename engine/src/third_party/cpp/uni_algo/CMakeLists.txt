# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(uni_algo_cppm STATIC)

target_sources(uni_algo_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        uni_algo.ixx)


find_package(uni-algo CONFIG REQUIRED)
target_link_libraries(uni_algo_cppm PUBLIC uni-algo::uni-algo)

add_library(uni-algo::cppm ALIAS uni_algo_cppm)

set_target_properties(uni_algo_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
