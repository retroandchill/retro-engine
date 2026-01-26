# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_vulkan STATIC)

target_sources(retro_vulkan PUBLIC
        FILE_SET CXX_MODULES FILES
        vulkan.cppm
        vulkan_video.cppm)


find_package(Vulkan REQUIRED)
target_link_libraries(retro_vulkan PUBLIC Vulkan::Vulkan)

set_target_properties(retro_vulkan PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
