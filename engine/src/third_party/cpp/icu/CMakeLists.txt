# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(icu_cppm STATIC)

target_sources(icu_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        icu.ixx)


find_package(ICU COMPONENTS uc i18n REQUIRED)
target_link_libraries(icu_cppm PUBLIC ICU::uc ICU::i18n)

add_library(ICU::Cppm ALIAS icu_cppm)

set_target_properties(icu_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
