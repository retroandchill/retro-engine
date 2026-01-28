# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(vulkan_cppm OBJECT)

target_sources(vulkan_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        vulkan.cppm
        vulkan_video.cppm)


find_package(Vulkan REQUIRED)
target_link_libraries(vulkan_cppm PUBLIC Vulkan::Vulkan)

add_library(Vulkan::Cppm ALIAS vulkan_cppm)

set_target_properties(vulkan_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
