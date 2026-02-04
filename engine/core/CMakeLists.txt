# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_CORE_SOURCES src/strings/name.cpp
        src/io/file_stream.cpp
        src/strings/name_c.cpp
        src/functional/delegate.cpp
        src/di/di.cpp
        src/io/stream.cpp
        src/io/buffered_stream.cpp
        src/async/task_scheduler.cpp
        src/async/manual_task_scheduler.cpp
)

set(RETRO_CORE_HEADERS include/retro/core/exports.h
        include/retro/core/per_project_boilerplate.hpp
        include/retro/core/strings/name.h
        include/retro/core/math/vector.h
        include/retro/core/math/color.h
        include/retro/core/macros.hpp)

set(RETRO_CORE_MODULES
        src/strings/format.ixx
        src/strings/name.ixx
        src/memory/buffers.ixx
        src/memory/ref_counted_ptr.ixx
        src/algorithm/hashing.ixx
        src/util/color.ixx
        src/io/file_stream.ixx
        src/functional/delegate.ixx
        src/memory/arena_allocator.ixx
        src/async/task.ixx
        src/containers/optional.ixx
        src/di/di.ixx
        src/di/metadata.ixx
        src/util/exceptions.ixx
        src/containers/inline_list.ixx
        src/c_api.ixx
        src/util/noncopyable.ixx
        src/type_traits/pointer.ixx
        src/type_traits/range.ixx
        src/type_traits/comparison.ixx
        src/type_traits/callable.ixx
        src/type_traits/variant.ixx
        src/type_traits/basic.ixx
        src/memory/align.ixx
        src/math/operations.ixx
        src/math/vector.ixx
        src/math/matrix.ixx
        src/math/transform.ixx
        src/functional/overload.ixx
        src/functional/binding.ixx
        src/io/stream.ixx
        src/io/buffered_stream.ixx
        src/async/task_scheduler.ixx
        src/async/manual_task_scheduler.ixx
        src/strings/cstring_view.ixx
        src/strings/encoding.ixx
        src/strings/deps/uni_algo.ixx
        src/containers/node_list.ixx
)

add_library(retro_core ${RETRO_CORE_SOURCES} ${RETRO_CORE_MODULES} ${RETRO_CORE_HEADERS})

retro_add_boilerplate(retro_core)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET retro_core_public_modules
    TYPE CXX_MODULES FILES
        ${RETRO_CORE_MODULES})

find_package(Boost REQUIRED COMPONENTS asio pool unordered)
find_package(fmt CONFIG REQUIRED)
find_package(uni-algo CONFIG REQUIRED)
find_package(gcem CONFIG REQUIRED)
target_link_libraries(retro_core PRIVATE Boost::asio Boost::unordered Boost::pool
                                        uni-algo::uni-algo
                                        fmt::fmt
                                        gcem)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")

if (BUILD_TESTS)
    add_subdirectory(test)
endif()
