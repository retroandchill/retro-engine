# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(retro_core src/strings/name.cpp
        src/filesystem/file_loader.cpp
)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/strings/concepts.ixx
        src/strings/index.ixx
        src/strings/cstring_view.ixx
        src/strings/literals.ixx
        src/strings/name.ixx
        src/strings/comparison.ixx
        src/memory/index.ixx
        src/memory/ref_counted_ptr.ixx
        src/defines.ixx
        src/algorithm/index.ixx
        src/algorithm/hashing.ixx
        src/basic_types.ixx
        src/filesystem/index.ixx
        src/filesystem/file_loader.ixx
        src/math/index.ixx
        src/math/concepts.ixx
        src/math/vector.ixx
        src/containers/index.ixx
        src/containers/packed_pool.ixx
        src/containers/polymorphic.ixx
        src/math/rounding.ixx
        src/memory/simple_arena.ixx)


target_link_libraries(retro_core PUBLIC retro_boost retro_cityhash retro_uni_algo retro_fmt)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")

if (BUILD_TESTS)
    add_subdirectory(test)
endif()
