# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_CORE_SOURCES private/src/strings/name.cpp
        private/src/io/file_stream.cpp
        private/src/strings/name_c.cpp
        private/src/functional/delegate.cpp
        private/src/di/service_provider.cpp
        private/src/io/stream.cpp
        private/src/io/buffered_stream.cpp
        private/src/async/task_scheduler.cpp
        private/src/async/manual_task_scheduler.cpp
        private/src/di/service_collection.cpp
        private/src/di/scoped_service_provider.cpp
        private/src/localization/text.cpp
        private/src/localization/text_history.cpp
        private/src/localization/localization_manager.cpp
)

set(RETRO_CORE_HEADERS public/include/retro/core/exports.h
        public/include/retro/core/per_project_boilerplate.hpp
        public/include/retro/core/strings/name_c.h
        public/include/retro/core/math/vector_c.h
        public/include/retro/core/math/color_c.h
        public/include/retro/core/macros.hpp)

set(RETRO_CORE_PRIVATE_MODULES
        private/src/di/scoped_service_provider.ixx
        private/src/localization/text_history.ixx
)

set(RETRO_CORE_PUBLIC_MODULES
        public/src/strings/format.ixx
        public/src/strings/name.ixx
        public/src/memory/buffers.ixx
        public/src/memory/ref_counted_ptr.ixx
        public/src/algorithm/hashing.ixx
        public/src/util/color.ixx
        public/src/io/file_stream.ixx
        public/src/functional/delegate.ixx
        public/src/memory/arena_allocator.ixx
        public/src/async/task.ixx
        public/src/containers/optional.ixx
        public/src/di/service_provider.ixx
        public/src/di/metadata.ixx
        public/src/util/exceptions.ixx
        public/src/containers/inline_list.ixx
        public/src/c_api.ixx
        public/src/util/noncopyable.ixx
        public/src/type_traits/pointer.ixx
        public/src/type_traits/range.ixx
        public/src/type_traits/comparison.ixx
        public/src/type_traits/callable.ixx
        public/src/type_traits/variant.ixx
        public/src/type_traits/basic.ixx
        public/src/memory/align.ixx
        public/src/math/operations.ixx
        public/src/math/vector.ixx
        public/src/math/matrix.ixx
        public/src/math/transform.ixx
        public/src/functional/overload.ixx
        public/src/functional/binding.ixx
        public/src/io/stream.ixx
        public/src/io/buffered_stream.ixx
        public/src/async/task_scheduler.ixx
        public/src/async/manual_task_scheduler.ixx
        public/src/strings/cstring_view.ixx
        public/src/strings/encoding.ixx
        public/src/strings/deps/uni_algo.ixx
        public/src/math/rect.ixx
        public/src/di/service_instance.ixx
        public/src/di/service_call_site.ixx
        public/src/di/service_identifier.ixx
        public/src/di/service_collection.ixx
        public/src/di.ixx
        public/src/util/deferred.ixx
        public/src/memory/small_unique_ptr.ixx
        public/src/di/service_scope.ixx
        public/src/localization/text_data.ixx
        public/src/localization/text_source_types.ixx
        public/src/util/enum_class_flags.ixx
        public/src/localization/text.ixx
        public/src/localization/loc_key.ixx
        public/src/util/date_time.ixx
        public/src/localization/text_key.ixx
        public/src/localization/string_table.ixx
        public/src/localization.ixx
        public/src/localization/localization_manager.ixx
        public/src/util/lazy_singleton.ixx
        public/src/localization/localized_text_source.ixx
        public/src/localization/text_localization_resource.ixx
)

add_library(retro_core ${RETRO_CORE_SOURCES} ${RETRO_CORE_PUBLIC_MODULES} ${RETRO_CORE_PRIVATE_MODULES} ${RETRO_CORE_HEADERS})

retro_add_boilerplate(retro_core)

target_include_directories(retro_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/public/include)

target_sources(retro_core PUBLIC
    FILE_SET retro_core_public_modules
    TYPE CXX_MODULES FILES
        ${RETRO_CORE_PUBLIC_MODULES})

target_sources(retro_core PRIVATE
        FILE_SET retro_core_private_modules
        TYPE CXX_MODULES FILES
        ${RETRO_CORE_PRIVATE_MODULES})

find_package(Boost REQUIRED COMPONENTS algorithm asio crc pool unordered)
find_package(fmt CONFIG REQUIRED)
find_package(uni-algo CONFIG REQUIRED)
find_package(gcem CONFIG REQUIRED)
find_package(ICU COMPONENTS uc i18n REQUIRED)
target_link_libraries(retro_core PRIVATE Boost::asio Boost::algorithm Boost::unordered Boost::pool
                                        Boost::crc
                                        uni-algo::uni-algo
                                        fmt::fmt
                                        gcem
                                        ICU::uc
                                        ICU::i18n)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")

if (BUILD_TESTS)
    add_subdirectory(test)
endif()
