cmake_minimum_required(VERSION 4.1)

set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD "d0edc3af-4c50-42ea-a356-e2862fe7a444")

option(RETRO_BUILD_SHARED "Build the engine as shared libraries" ON)

option(BUILD_TESTS "Should the test cases be built as well" ON)

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
endif()

project(retro_engine LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_MODULE_STD ON)
if (MSVC)
    add_compile_definitions(_UTF8)
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

add_custom_target(retro_managed ALL
        COMMAND dotnet build "${RETRO_MANAGED_SOLUTION}"
            -c "${DOTNET_CONFIGURATION}"
            -p:Platform="Any CPU"
            -p:OutputPath="${RETRO_BIN_DIR}/"
            -p:RetroEngineBackend="Native"
        WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
        USES_TERMINAL
        COMMENT "Building .NET managed assemblies")

if (BUILD_TESTS)
    include(CTest)
    enable_testing()
endif()

add_subdirectory(engine)
add_subdirectory(game)