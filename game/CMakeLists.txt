add_executable(retro_game
    src/main.cpp
)

target_link_libraries(retro_game PRIVATE retro_platform)

find_package(SDL3 CONFIG REQUIRED)
target_link_libraries(retro_game PUBLIC SDL3::SDL3)

if(RETRO_BUILD_SHARED)
    add_custom_command(TARGET retro_game POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_if_different
            $<TARGET_FILE:retro_platform>
            $<TARGET_FILE_DIR:retro_game>
            COMMENT "Copying engine DLLs to game output directory"
    )
endif()