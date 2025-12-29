//
// Created by fcors on 12/29/2025.
//
module;

#define BOOST_DLL_USE_STD_FS
#define BOOST_DLL_USE_BOOST_SHARED_PTR
#include <boost/dll.hpp>

export module boost;

namespace boost::dll
{
    export using dll::shared_library;
    export using dll::import_symbol;
    export using dll::library_info;
    export using dll::symbol_location_ptr;
    export using dll::symbol_location;
    export using dll::program_location;
    export using dll::operator==;
    export using dll::operator!=;
    export using dll::swap;
}