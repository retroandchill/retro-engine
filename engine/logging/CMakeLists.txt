add_library(retro_logging SHARED )

target_sources(retro_logging PUBLIC
        FILE_SET CXX_MODULES FILES
        src/index.ixx)

target_link_libraries(retro_logging PUBLIC retro_fmt)

set_target_properties(retro_logging PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
