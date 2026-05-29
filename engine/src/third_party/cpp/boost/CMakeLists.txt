# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(BOOST_CPPM_MODULES pool.ixx
        unordered.ixx
)

add_library(boot_cppm STATIC
        ${BOOST_CPPM_MODULES})

target_sources(boot_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        ${BOOST_CPPM_MODULES})


find_package(Boost REQUIRED COMPONENTS pool unordered)
target_link_libraries(boot_cppm PUBLIC Boost::pool Boost::unordered)

add_library(Boost::cppm ALIAS boot_cppm)

set_target_properties(boot_cppm PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
