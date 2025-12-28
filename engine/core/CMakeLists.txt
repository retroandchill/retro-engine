add_library(retro_core SHARED src/strings/name.cpp
        src/filesystem/file_loader.cpp)

target_include_directories(retro_core PUBLIC include)

target_sources(retro_core PUBLIC
    FILE_SET CXX_MODULES FILES
        src/index.ixx
        src/strings/concepts.ixx
        src/strings/index.ixx
        src/strings/cstring_view.ixx
        src/strings/literals.ixx
        src/strings/name.ixx
        src/strings/comparison.ixx
        src/memory/index.ixx
        src/memory/ref_counted_ptr.ixx
        src/defines.ixx
        src/algorithm/index.ixx
        src/algorithm/hashing.ixx
        src/basic_types.ixx
        src/filesystem/index.ixx
        src/filesystem/file_loader.ixx)


find_package(fmt CONFIG REQUIRED)
target_link_libraries(retro_core PUBLIC fmt::fmt)

set_target_properties(retro_core PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")

if (BUILD_TESTS)
    add_subdirectory(test)
endif()