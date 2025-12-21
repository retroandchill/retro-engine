add_library(retro_scripting)

if(NOT RETRO_BUILD_SHARED)
    target_compile_definitions(retro_scripting PUBLIC RETRO_STATIC)
endif()

target_sources(retro_scripting PUBLIC
        FILE_SET CXX_MODULES FILES
        src/dotnet.ixx)

target_link_libraries(retro_scripting PUBLIC
        retro_core
        dotnet_host)