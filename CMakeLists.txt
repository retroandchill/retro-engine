# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
cmake_minimum_required(VERSION 4.1)

set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD "d0edc3af-4c50-42ea-a356-e2862fe7a444")

option(RETRO_BUILD_SHARED "Build the engine as shared libraries" ON)

option(BUILD_TESTS "Should the test cases be built as well" ON)
option(RETRO_WITH_CASE_PRESERVING_NAME "Should the Name type have an extra field for the case" ON)

if(NOT RETRO_BUILD_SHARED)
    set(BUILD_SHARED_LIBS OFF)

    # Configure vcpkg to use static triplets
    if(WIN32)
        set(VCPKG_TARGET_TRIPLET "x64-windows-static" CACHE STRING "")
        set(VCPKG_HOST_TRIPLET "x64-windows-static" CACHE STRING "")
    elseif(APPLE)
        set(VCPKG_TARGET_TRIPLET "x64-osx-static" CACHE STRING "")
    else()
        set(VCPKG_TARGET_TRIPLET "x64-linux-static" CACHE STRING "")
    endif()
else()
    set(BUILD_SHARED_LIBS ON)
endif()

set(VCPKG_DEPS_DIR ${CMAKE_BINARY_DIR}/vcpkg_installed/${VCPKG_TARGET_TRIPLET})
set(VCPKG_BIN_DIR ${VCPKG_DEPS_DIR}/bin)

project(retro_engine LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_MODULE_STD ON)
if (MSVC)
    add_compile_definitions(_UTF8)
endif()

if (RETRO_WITH_CASE_PRESERVING_NAME)
    add_compile_definitions(RETRO_WITH_CASE_PRESERVING_NAME=1)
    set(RETRO_MANAGED_OPTIONS -p:RetroEngineCasePreservingNames="true")
else()
    add_compile_definitions(RETRO_WITH_CASE_PRESERVING_NAME=0)
    set(RETRO_MANAGED_OPTIONS )
endif()

set(RETRO_OUTPUT_ROOT "${CMAKE_BINARY_DIR}")

set(RETRO_BIN_DIR "${RETRO_OUTPUT_ROOT}/bin")
set(RETRO_LIB_DIR "${RETRO_OUTPUT_ROOT}/lib")

set(RETRO_BIN_DIR "${RETRO_BIN_DIR}" CACHE PATH "Retro Engine binary output directory")
set(RETRO_LIB_DIR "${RETRO_LIB_DIR}" CACHE PATH "Retro Engine library output directory")

set(RETRO_MANAGED_SOLUTION "${CMAKE_SOURCE_DIR}/RetroEngine.Managed.sln")

if(CMAKE_CONFIGURATION_TYPES)  # multi-config generators (Visual Studio, etc.)
    # Map any non-Debug config to Release for dotnet
    set(DOTNET_CONFIGURATION "$<IF:$<CONFIG:Debug>,Debug,Release>")
else()  # single-config generators (Ninja, Makefiles, typical CLion setup)
    if(CMAKE_BUILD_TYPE STREQUAL "Debug")
        set(DOTNET_CONFIGURATION "Debug")
    else()
        set(DOTNET_CONFIGURATION "Release")
    endif()
endif()

find_program(GLSLC_EXECUTABLE glslc HINTS "$ENV{VULKAN_SDK}/Bin" "$ENV{VULKAN_SDK}/Bin32")
if (NOT GLSLC_EXECUTABLE)
    message(FATAL_ERROR "glslc not found. Install Vulkan SDK or add glslc to PATH.")
endif ()

function(retro_add_boilerplate TARGET_NAME)
    set(BOILERPLATE_FILE "${CMAKE_CURRENT_BINARY_DIR}/${TARGET_NAME}_boilerplate.cpp")
    configure_file(
            "${CMAKE_SOURCE_DIR}/config/per_project_boilerplate.cpp.in"
            "${BOILERPLATE_FILE}"
            @ONLY
    )
    target_sources(${TARGET_NAME} PRIVATE "${BOILERPLATE_FILE}")
endfunction()

add_custom_target(retro_managed ALL
        COMMAND dotnet build "${RETRO_MANAGED_SOLUTION}"
            -c "${DOTNET_CONFIGURATION}"
            -p:Platform="Any CPU"
            -p:RetroEngineNativeOutputDirectory="${RETRO_BIN_DIR}/"
            -p:RetroEngineBackend="Native"
            ${RETRO_MANAGED_OPTIONS}
        WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
        USES_TERMINAL
        COMMENT "Building .NET managed assemblies")

if (BUILD_TESTS)
    include(CTest)
    enable_testing()
endif()

add_subdirectory(external)
add_subdirectory(engine)
add_subdirectory(game)
