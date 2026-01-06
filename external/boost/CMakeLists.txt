# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_boost STATIC)

target_sources(retro_boost PUBLIC
        FILE_SET CXX_MODULES FILES
        boost.ixx)


find_package(Boost REQUIRED COMPONENTS pool unordered dll uuid optional)
target_link_libraries(retro_boost PUBLIC Boost::pool Boost::unordered Boost::dll Boost::uuid Boost::optional)

set_target_properties(retro_boost PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
