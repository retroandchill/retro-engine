add_library(retro_platform
        src/exports.h)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_platform PUBLIC RETRO_STATIC)
endif()

target_sources(retro_platform PUBLIC
        FILE_SET CXX_MODULES FILES
        src/window.ixx
        src/platform.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_platform PUBLIC SDL3::SDL3)