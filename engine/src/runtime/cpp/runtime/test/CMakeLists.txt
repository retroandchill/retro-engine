# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
SET(RETRO_RUNTIME_TEST_HEADERS )

SET(RETRO_RUNTIME_TEST_MODULES )

SET(RETRO_RUNTIME_TEST_SOURCES
        rendering/text/font_service_test.cpp
)

add_executable(retro_runtime_tests ${RETRO_RUNTIME_TEST_SOURCES} ${RETRO_RUNTIME_TEST_HEADERS} ${RETRO_RUNTIME_TEST_MODULES})

retro_add_boilerplate(retro_runtime_tests)

target_include_directories(retro_runtime_tests PRIVATE .)

set(EMBED_BINARY_FILES_TEMPLATE ${CMAKE_SOURCE_DIR}/templates/embedded_file.ixx.mustache)
set(OUTPUT_EMBED_FILE "${CMAKE_CURRENT_BINARY_DIR}/files.ixx")

retro_embed_files(retro_test_fonts "${OUTPUT_EMBED_FILE}" "${EMBED_BINARY_FILES_TEMPLATE}"
        SOURCE_FILES
            "${CMAKE_CURRENT_SOURCE_DIR}/resources/test_font.ttf"
        COMMENT "Embedding binary file dependencies")

target_sources(retro_runtime_tests PUBLIC
        FILE_SET retro_test_public
        TYPE CXX_MODULES
        FILES
        ${RETRO_RUNTIME_TEST_MODULES})

target_sources(retro_runtime_tests PRIVATE
        FILE_SET retro_runtime_tests_private
        TYPE CXX_MODULES
        BASE_DIRS ${CMAKE_CURRENT_SOURCE_DIR} ${CMAKE_CURRENT_BINARY_DIR}
        FILES
            "${OUTPUT_EMBED_FILE}")

add_dependencies(retro_runtime_tests retro_test_fonts)

find_package(GTest CONFIG REQUIRED)
target_link_libraries(retro_runtime_tests PRIVATE
        retro_runtime
        Freetype::Freetype
        msdfgen::msdfgen
        msdfgen::msdfgen-ext
        msdf-atlas-gen::msdf-atlas-gen
        GTest::gtest GTest::gtest_main GTest::gmock GTest::gmock_main
)

gtest_add_tests(TARGET retro_runtime_tests WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR})

set_target_properties(retro_runtime_tests PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
