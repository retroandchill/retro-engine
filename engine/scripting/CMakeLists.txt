add_library(retro_scripting)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_scripting PUBLIC RETRO_STATIC)
endif()

target_sources(retro_scripting PUBLIC
        FILE_SET CXX_MODULES FILES
        src/dotnet.ixx)

find_package(SDL3 CONFIG REQUIRED)
find_package(unofficial-nethost CONFIG REQUIRED)
target_link_libraries(retro_scripting PUBLIC
        retro_core
        retro_platform
        unofficial::nethost::nethost
        SDL3::SDL3)