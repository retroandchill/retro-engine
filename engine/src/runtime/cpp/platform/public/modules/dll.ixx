/**
 * @file dll.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define BOOST_DLL_USE_STD_FS
#include <boost/dll.hpp>
#include <boost/system/error_code.hpp>
#include <utility>

export module retro.platform.dll;

import std;

import retro.core.strings.cstring_view;

namespace retro
{
    export [[nodiscard]] inline std::filesystem::path get_executable_path()
    {
        return boost::dll::program_location().parent_path();
    }

    export enum class LibraryLoadMode
    {
        default_mode = boost::dll::load_mode::default_mode,
        dont_resolve_dll_references = boost::dll::load_mode::dont_resolve_dll_references,
        load_ignore_code_authz_level = boost::dll::load_mode::load_ignore_code_authz_level,
        load_with_altered_search_path = boost::dll::load_mode::load_with_altered_search_path,
        rtld_lazy = boost::dll::load_mode::rtld_lazy,
        rtld_now = boost::dll::load_mode::rtld_now,
        rtdl_global = boost::dll::load_mode::rtld_global,
        rtdl_local = boost::dll::load_mode::rtld_local,
        rtdl_deepbind = boost::dll::load_mode::rtld_deepbind,
        rtdl_append_decorations = boost::dll::load_mode::append_decorations,
        search_system_folders = boost::dll::load_mode::search_system_folders
    };

    export enum class LibraryLoadError
    {
        not_loaded = 0,
        not_found,
        access_denied,
        bad_format,
        invalid_path,
        unknown,
    };

    [[nodiscard]] inline LibraryLoadError translate_library_load_error(const boost::system::error_code &ec) noexcept
    {
        using enum LibraryLoadError;
        if (!ec)
            return unknown; // should not happen; caller should only translate on failure

        // Use portable errc mappings when possible. Note: many platforms report "dependency missing"
        // as the same error as "module not found".
        namespace errc = boost::system::errc;

        if (ec == errc::no_such_file_or_directory)
            return not_found;

        if (ec == errc::permission_denied)
            return access_denied;

        if (ec == errc::executable_format_error)
            return bad_format;

        if (ec == errc::invalid_argument)
            return invalid_path;

        return unknown;
    }

    export class SharedLibrary
    {
      public:
        using NativeHandle = boost::dll::shared_library::native_handle_t;

        SharedLibrary() = default;

        static inline std::expected<SharedLibrary, LibraryLoadError> create(
            const std::filesystem::path &path,
            LibraryLoadMode load_mode = LibraryLoadMode::default_mode)
        {
            boost::system::error_code ec;
            boost::dll::shared_library library{path,
                                               static_cast<boost::dll::load_mode::type>(std::to_underlying(load_mode)),
                                               ec};
            if (ec.failed())
            {
                return std::unexpected(translate_library_load_error(ec));
            }

            return SharedLibrary{std::move(library)};
        }

        inline std::expected<void, LibraryLoadError> load(
            const std::filesystem::path &path,
            const LibraryLoadMode load_mode = LibraryLoadMode::default_mode)
        {
            boost::system::error_code ec;
            library_.load(path, static_cast<boost::dll::load_mode::type>(std::to_underlying(load_mode)), ec);
            if (ec.failed())
            {
                return std::unexpected(translate_library_load_error(ec));
            }

            return {};
        }

        inline void unload()
        {
            library_.unload();
        }

        [[nodiscard]] inline bool is_loaded() const noexcept
        {
            return library_.is_loaded();
        }

        [[nodiscard]] inline bool has(const CStringView view) const
        {
            return library_.has(view.data());
        }

        template <typename T>
        decltype(auto) get(const CStringView view) const
        {
            return library_.get<T>(view.data());
        }

        template <typename T>
        decltype(auto) get_alias(const CStringView view) const
        {
            return library_.get_alias<T>(view.data());
        }

        [[nodiscard]] inline NativeHandle native() const noexcept
        {
            return library_.native();
        }

        [[nodiscard]] inline std::filesystem::path path() const noexcept
        {
            return library_.location();
        }

      private:
        explicit inline SharedLibrary(boost::dll::shared_library &&library) noexcept : library_(std::move(library))
        {
        }

        boost::dll::shared_library library_;
    };

} // namespace retro
