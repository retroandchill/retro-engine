add_executable(retro_core_tests strings/test_name.cpp
        math/test_vector.cpp)

target_include_directories(retro_core_tests PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

find_package(Catch2 CONFIG REQUIRED)
target_link_libraries(retro_core_tests PRIVATE
        retro_core
        Catch2::Catch2WithMain
)

set_target_properties(retro_core_tests PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")