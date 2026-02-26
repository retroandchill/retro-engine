# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_CORE_SOURCES private/strings/name.cpp
        private/io/file_stream.cpp
        private/strings/name_c.cpp
        private/functional/delegate.cpp
        private/di/service_provider.cpp
        private/io/stream.cpp
        private/io/buffered_stream.cpp
        private/async/task_scheduler.cpp
        private/async/manual_task_scheduler.cpp
        private/di/service_collection.cpp
        private/di/scoped_service_provider.cpp
        private/localization/locale_c.cpp
        private/localization/decimal_format_c.cpp
        private/localization/date_format_c.cpp
        private/localization/calendar_c.cpp
        private/localization/time_zone_c.cpp
        private/localization/collator_c.cpp
        private/localization/plural_rules_c.cpp
)

set(RETRO_CORE_HEADERS public/include/retro/core/exports.h
        public/include/retro/core/per_project_boilerplate.hpp
        public/include/retro/core/macros.hpp
)

set(RETRO_CORE_PRIVATE_MODULES
        private/di/scoped_service_provider.ixx
        private/localization/icu.ixx
        private/localization/buffers.ixx
)

set(RETRO_CORE_PUBLIC_MODULES
        public/modules/strings/format.ixx
        public/modules/strings/name.ixx
        public/modules/memory/buffers.ixx
        public/modules/memory/ref_counted_ptr.ixx
        public/modules/algorithm/hashing.ixx
        public/modules/util/color.ixx
        public/modules/io/file_stream.ixx
        public/modules/functional/delegate.ixx
        public/modules/memory/arena_allocator.ixx
        public/modules/async/task.ixx
        public/modules/containers/optional.ixx
        public/modules/di/service_provider.ixx
        public/modules/di/metadata.ixx
        public/modules/util/exceptions.ixx
        public/modules/containers/inline_list.ixx
        public/modules/util/noncopyable.ixx
        public/modules/type_traits/pointer.ixx
        public/modules/type_traits/range.ixx
        public/modules/type_traits/comparison.ixx
        public/modules/type_traits/callable.ixx
        public/modules/type_traits/variant.ixx
        public/modules/type_traits/basic.ixx
        public/modules/memory/align.ixx
        public/modules/math/operations.ixx
        public/modules/math/vector.ixx
        public/modules/math/matrix.ixx
        public/modules/math/transform.ixx
        public/modules/functional/overload.ixx
        public/modules/functional/binding.ixx
        public/modules/io/stream.ixx
        public/modules/io/buffered_stream.ixx
        public/modules/async/task_scheduler.ixx
        public/modules/async/manual_task_scheduler.ixx
        public/modules/strings/cstring_view.ixx
        public/modules/strings/encoding.ixx
        public/modules/strings/deps/uni_algo.ixx
        public/modules/math/rect.ixx
        public/modules/di/service_instance.ixx
        public/modules/di/service_call_site.ixx
        public/modules/di/service_identifier.ixx
        public/modules/di/service_collection.ixx
        public/modules/di.ixx
        public/modules/util/deferred.ixx
        public/modules/memory/small_unique_ptr.ixx
        public/modules/di/service_scope.ixx
        public/modules/util/enum_class_flags.ixx
        public/modules/functional/function_ref.ixx
)

add_library(retro_core ${RETRO_CORE_SOURCES} ${RETRO_CORE_PUBLIC_MODULES} ${RETRO_CORE_PRIVATE_MODULES} ${RETRO_CORE_HEADERS})

retro_add_boilerplate(retro_core)

target_include_directories(retro_core PUBLIC public/include)

target_sources(retro_core PUBLIC
    FILE_SET retro_core_public_modules
    TYPE CXX_MODULES FILES
        ${RETRO_CORE_PUBLIC_MODULES})

target_sources(retro_core PRIVATE
        FILE_SET retro_core_private_modules
        TYPE CXX_MODULES FILES
        ${RETRO_CORE_PRIVATE_MODULES})

find_package(Boost REQUIRED COMPONENTS algorithm asio crc pool unordered uuid)
find_package(fmt CONFIG REQUIRED)
find_package(uni-algo CONFIG REQUIRED)
find_package(gcem CONFIG REQUIRED)
find_package(ICU COMPONENTS uc i18n REQUIRED)
target_link_libraries(retro_core PRIVATE Boost::asio Boost::algorithm Boost::unordered Boost::pool
                                        Boost::crc
                                        Boost::uuid
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
