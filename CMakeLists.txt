cmake_minimum_required(VERSION 4.1)

set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD "d0edc3af-4c50-42ea-a356-e2862fe7a444")

option(RETRO_BUILD_SHARED "Build the engine as shared libraries" ON)

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

add_subdirectory(third_party)
add_subdirectory(engine)
add_subdirectory(game)