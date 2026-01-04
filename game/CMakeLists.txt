# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
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
