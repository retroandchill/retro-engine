#----------------------------------------------------------------
# Generated CMake target import file for configuration "Debug".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "unofficial::nethost::nethost" for configuration "Debug"
set_property(TARGET unofficial::nethost::nethost APPEND PROPERTY IMPORTED_CONFIGURATIONS DEBUG)
set_target_properties(unofficial::nethost::nethost PROPERTIES
  IMPORTED_IMPLIB_DEBUG "${_IMPORT_PREFIX}/debug/lib/nethost.lib"
  IMPORTED_LOCATION_DEBUG "${_IMPORT_PREFIX}/debug/bin/nethost.dll"
  )

list(APPEND _cmake_import_check_targets unofficial::nethost::nethost )
list(APPEND _cmake_import_check_files_for_unofficial::nethost::nethost "${_IMPORT_PREFIX}/debug/lib/nethost.lib" "${_IMPORT_PREFIX}/debug/bin/nethost.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
