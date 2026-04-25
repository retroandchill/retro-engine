# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
cmake_minimum_required(VERSION 4.1)

set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD "d0edc3af-4c50-42ea-a356-e2862fe7a444")

option(RETRO_BUILD_SHARED "Build the engine as shared libraries" ON)

option(BUILD_TESTS "Should the test cases be built as well" ON)
option(RETRO_WITH_EDITOR_DATA "Is this a build that expects to include the editor" ON)
option(RETRO_WITH_CASE_PRESERVING_NAME "Should the Name type have an extra field for the case" ON)

set(INTERMEDIATE_LIB_TYPE OBJECT)
set(BUILD_SHARED_LIBS OFF)

set(VCPKG_CRT_LINKAGE static)
set(VCPKG_LIBRARY_LINKAGE static)

set(VCPKG_DEPS_DIR ${CMAKE_BINARY_DIR}/vcpkg_installed/${VCPKG_TARGET_TRIPLET})
set(VCPKG_BIN_DIR ${VCPKG_DEPS_DIR}/bin)

if(WIN32)
    set(RETRO_VENV_PYTHON "${CMAKE_SOURCE_DIR}/.venv/Scripts/python.exe")
else()
    set(RETRO_VENV_PYTHON "${CMAKE_SOURCE_DIR}/.venv/bin/python")
endif()

if(EXISTS "${RETRO_VENV_PYTHON}")
    set(Python3_EXECUTABLE "${RETRO_VENV_PYTHON}" CACHE FILEPATH "Python interpreter from project virtual environment")
endif()

project(retro_engine LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_MODULE_STD ON)
if (MSVC)
    add_compile_definitions(_UTF8)
endif()

if (RETRO_WITH_EDITOR_DATA)
    add_compile_definitions(RETRO_WITH_EDITOR_DATA=1)
else()
    add_compile_definitions(RETRO_WITH_EDITOR_DATA=0)
endif()


if (RETRO_WITH_CASE_PRESERVING_NAMES)
    add_compile_definitions(RETRO_WITH_CASE_PRESERVING_NAME=1)
else()
    add_compile_definitions(RETRO_WITH_CASE_PRESERVING_NAME=0)
endif()

string(TOLOWER ${CMAKE_BUILD_TYPE} RETRO_BUILD_CONFIG)
set(RETRO_OUTPUT_ROOT "${CMAKE_SOURCE_DIR}/engine")

string(TOLOWER ${CMAKE_SYSTEM_NAME} RETRO_SYSTEM_NAME)
if (${RETRO_SYSTEM_NAME} STREQUAL "windows")
    set(RETRO_SYSTEM_NAME "win")
endif()

set(NORMALIZED_PROCESSOR "${CMAKE_SYSTEM_PROCESSOR}")
if(NORMALIZED_PROCESSOR MATCHES "AMD64|x86_64|amd64")
    set(NORMALIZED_PROCESSOR "x64")
elseif(NORMALIZED_PROCESSOR MATCHES "ARM64|aarch64")
    set(NORMALIZED_PROCESSOR "arm64")
elseif(NORMALIZED_PROCESSOR MATCHES "X86|i686")
    set(NORMALIZED_PROCESSOR "x86")
endif()

set(RETRO_TARGET_PLATFORM "${RETRO_SYSTEM_NAME}-${NORMALIZED_PROCESSOR}")

set(RETRO_BIN_DIR "${RETRO_OUTPUT_ROOT}/bin/${RETRO_BUILD_CONFIG}/runtimes/${RETRO_TARGET_PLATFORM}/native")
set(RETRO_BIN_DIR "${RETRO_BIN_DIR}" CACHE PATH "Retro Engine binary output directory")

find_program(GLSLC_EXECUTABLE glslc HINTS "$ENV{VULKAN_SDK}/Bin" "$ENV{VULKAN_SDK}/Bin32")
if (NOT GLSLC_EXECUTABLE)
    message(FATAL_ERROR "glslc not found. Install Vulkan SDK or add glslc to PATH.")
endif ()

function(retro_add_boilerplate TARGET_NAME)
    get_target_property(TARGET_TYPE ${TARGET_NAME} TYPE)

    if(TARGET_TYPE STREQUAL "EXECUTABLE" OR TARGET_TYPE STREQUAL "SHARED_LIBRARY")
        set(BOILERPLATE_FILE "${CMAKE_CURRENT_BINARY_DIR}/${TARGET_NAME}_boilerplate.cpp")
        configure_file(
                "${CMAKE_SOURCE_DIR}/templates/per_project_boilerplate.cpp.in"
                "${BOILERPLATE_FILE}"
                @ONLY
        )
        target_sources(${TARGET_NAME} PRIVATE "${BOILERPLATE_FILE}")
    endif()
endfunction()

if (BUILD_TESTS)
    include(CTest)
    include(GoogleTest)
    enable_testing()
endif()

add_subdirectory(engine)
