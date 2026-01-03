add_executable(retro_game
    src/main.cpp
)

target_link_libraries(retro_game
        PRIVATE
            retro_scripting
            retro_renderer
            retro_sdl
            retro_interop
            retro_logging)

set_target_properties(retro_game PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")