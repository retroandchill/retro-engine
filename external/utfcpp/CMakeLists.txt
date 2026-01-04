# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_utfcpp STATIC)

target_sources(retro_utfcpp PUBLIC
        FILE_SET CXX_MODULES FILES
        utfcpp.ixx)


find_package(utf8cpp REQUIRED)
target_link_libraries(retro_utfcpp PUBLIC utf8::cpp utf8cpp::utf8cpp)

set_target_properties(retro_utfcpp PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
