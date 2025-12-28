add_library(retro_vulkan STATIC)

target_sources(retro_vulkan PUBLIC
        FILE_SET CXX_MODULES FILES
        vulkan.cppm)


find_package(Vulkan REQUIRED)
target_link_libraries(retro_vulkan PUBLIC Vulkan::Vulkan)

set_target_properties(retro_vulkan PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
