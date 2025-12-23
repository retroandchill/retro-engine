add_executable(retro_game
    src/main.cpp
)

find_package(SDL3 CONFIG REQUIRED)

target_link_libraries(retro_game
        PRIVATE
            retro_runtime
            SDL3::SDL3)

set_target_properties(retro_game PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")