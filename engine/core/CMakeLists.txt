# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_CORE_SOURCES src/strings/name.cpp
        src/platform/io.cpp
        src/strings/name_c.cpp
        src/functional/functional.cpp
        src/async/async.cpp
        src/platform/platform.cpp
)

set(RETRO_CORE_HEADERS include/retro/core/exports.h
        include/retro/core/per_project_boilerplate.hpp
        include/retro/core/strings/name.h
        include/retro/core/math/vector.h
        include/retro/core/math/color.h
        include/retro/core/macros.hpp)

set(RETRO_CORE_MODULES src/core.ixx
        src/strings/strings.ixx
        src/strings/name.ixx
        src/memory/memory.ixx
        src/memory/ref_counted_ptr.ixx
        src/defines.ixx
        src/algorithm.ixx
        src/basic_types.ixx
        src/platform/io.ixx
        src/math.ixx
        src/functional/functional.ixx
        src/memory/arena_allocator.ixx
        src/concepts.ixx
        src/async/async.ixx
        src/platform/platform.ixx
        src/containers/optional.ixx
)

add_library(retro_core ${RETRO_CORE_SOURCES} ${RETRO_CORE_MODULES} ${RETRO_CORE_HEADERS})

retro_add_boilerplate(retro_core)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        ${RETRO_CORE_MODULES})

find_package(Boost REQUIRED COMPONENTS asio dll pool unordered)
find_package(fmt CONFIG REQUIRED)
find_package(uni-algo CONFIG REQUIRED)
target_link_libraries(retro_core PUBLIC Boost::dll
                                 PRIVATE Boost::asio Boost::unordered Boost::pool
                                        uni-algo::uni-algo
                                        fmt::fmt)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")

if (BUILD_TESTS)
    add_subdirectory(test)
endif()
