add_library(retro_boost STATIC)

target_sources(retro_boost PUBLIC
        FILE_SET CXX_MODULES FILES
        boost.ixx)


find_package(Boost REQUIRED COMPONENTS dll uuid)
target_link_libraries(retro_boost PUBLIC Boost::dll Boost::uuid)

set_target_properties(retro_boost PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
