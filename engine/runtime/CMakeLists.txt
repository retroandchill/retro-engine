add_library(retro_runtime)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_runtime PUBLIC RETRO_STATIC)
endif()

target_sources(retro_runtime PUBLIC
        FILE_SET CXX_MODULES FILES
        src/engine.ixx)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_runtime PUBLIC
        retro_core
        retro_platform)