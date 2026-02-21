# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
SET(RETRO_CORE_TEST_HEADERS gtest_helpers.hpp containers/inline_list/gtest_setup.hpp
        containers/optional/test_utilities.hpp)

SET(RETRO_CORE_TEST_MODULES containers/optional/test_types.ixx)

SET(RETRO_CORE_TEST_SOURCES strings/test_name.cpp
        math/test_vector.cpp
        memory/test_ref_counted_ptr.cpp
        functional/test_delegates.cpp
        async/test_tasks.cpp
        containers/inline_list/compare_test.cpp
        containers/inline_list/constexpr_test.cpp
        containers/inline_list/constructors_test.cpp
        containers/inline_list/container_requirements_test.cpp
        containers/inline_list/erasure_test.cpp
        containers/inline_list/modifiers_test.cpp
        containers/inline_list/size_n_data_test.cpp
        containers/inline_list/triviality_test.cpp
        containers/optional/optional_test.cpp
        containers/optional/optional_constexpr_test.cpp
        containers/optional/optional_monadic_test.cpp
        containers/optional/optional_range_support_test.cpp
        containers/optional/optional_ref_test.cpp
        containers/optional/optional_ref_monadic_test.cpp
        memory/test_small_unique_ptr.cpp
        localization/test_plural_rules.cpp
)

add_executable(retro_core_tests ${RETRO_CORE_TEST_SOURCES} ${RETRO_CORE_TEST_HEADERS} ${RETRO_CORE_TEST_MODULES})

retro_add_boilerplate(retro_core_tests)

target_include_directories(retro_core_tests PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

target_sources(retro_core_tests PUBLIC
        FILE_SET CXX_MODULES FILES
        ${RETRO_CORE_TEST_MODULES})

find_package(GTest CONFIG REQUIRED)
target_link_libraries(retro_core_tests PRIVATE
        retro_core
        GTest::gtest GTest::gtest_main GTest::gmock GTest::gmock_main
)

if(MSVC)
    target_compile_options(retro_core_tests PRIVATE /bigobj)
endif()

set_target_properties(retro_core_tests PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
