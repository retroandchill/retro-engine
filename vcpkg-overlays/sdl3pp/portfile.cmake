vcpkg_from_github(
        OUT_SOURCE_PATH SOURCE_PATH
        REPO talesm/SDL3pp
        REF "${VERSION}"
        SHA512 c579e2a7d60e56bfb0ecdcccef7ed4c1fd584472ecf0deb9d4c2ca5958bdf0f462132b6c4fef26c3abdc015e4642068a67487a28227bacb69c4cbacc3af9972c
        HEAD_REF main
)

vcpkg_cmake_configure(
        SOURCE_PATH "${SOURCE_PATH}"
        OPTIONS
        -DBUILD_EXAMPLES=OFF
        -DSDL3PP_BUILD_EXAMPLES_AGAINST_AMALGAMATION=OFF
)

vcpkg_cmake_install()

file(INSTALL "${SOURCE_PATH}/include/SDL3pp" DESTINATION "${CURRENT_PACKAGES_DIR}/include/")
file(INSTALL "${CMAKE_CURRENT_LIST_DIR}/usage" DESTINATION "${CURRENT_PACKAGES_DIR}/share/${PORT}")
vcpkg_install_copyright(FILE_LIST "${SOURCE_PATH}/LICENSE.txt")
