add_library(retro_platform
        src/loading/shared_library.cpp
        src/platform.cpp
        src/filesystem/filesystem.cpp)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_platform PUBLIC RETRO_STATIC)
endif()

target_sources(retro_platform PUBLIC
        FILE_SET CXX_MODULES FILES
        src/window.ixx
        src/platform.ixx
        src/loading/loading.ixx
        src/loading/shared_library.ixx
        src/filesystem/filesystem.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_platform PUBLIC
        retro_core
        SDL3::SDL3)