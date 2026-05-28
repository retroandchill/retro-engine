# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

add_library(msdf_atlas_gen_cppm OBJECT)

target_sources(msdf_atlas_gen_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        msdf_atlas_gen.ixx)


find_package(msdf-atlas-gen CONFIG REQUIRED)
target_link_libraries(msdf_atlas_gen_cppm PUBLIC msdf-atlas-gen::msdf-atlas-gen)

add_library(msdf-atlas-gen::cppm ALIAS msdf_atlas_gen_cppm)

set_target_properties(msdf_atlas_gen_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
