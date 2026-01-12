# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(RETRO_LOGGING_SOURCES src/logger.cpp
        src/logger_c.cpp)

set(RETRO_LOGGING_MODULES src/index.ixx
        src/log_level.ixx
        src/logger.ixx)

set(RETRO_LOGGING_HEADERS include/retro/logging/logger.h)

add_library(retro_logging ${RETRO_LOGGING_SOURCES} ${RETRO_LOGGING_MODULES} ${RETRO_LOGGING_HEADERS})

retro_add_boilerplate(retro_logging)

target_sources(retro_logging PUBLIC
        FILE_SET CXX_MODULES FILES
        ${RETRO_LOGGING_MODULES})

target_include_directories(retro_logging PUBLIC include)

target_link_libraries(retro_logging PUBLIC retro_core retro_fmt retro_spdlog)

set_target_properties(retro_logging PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
