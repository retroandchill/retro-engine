# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_uni_algo STATIC)

target_sources(retro_uni_algo PUBLIC
        FILE_SET CXX_MODULES FILES
        uni_algo.ixx)


find_package(uni-algo CONFIG REQUIRED)
target_link_libraries(retro_uni_algo PUBLIC uni-algo::uni-algo)

set_target_properties(retro_uni_algo PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
