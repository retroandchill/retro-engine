/**
 * @file freetype.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.rendering.text.font:freetype;

import :dependencies;
import std;
import retro.core.util.exceptions;

namespace retro
{
    inline void throw_freetype_error(const FT_Error error, std::string_view message)
    {
        if (error != FT_Err_Ok)
        {
            throw PlatformException{std::format("{}: {}", message, FT_Error_String(error))};
        }
    }

    struct FreeTypeFaceDeleter final
    {
        inline void operator()(FT_Face face) const noexcept
        {
            FT_Done_Face(face);
        }
    };

    class FreeTypeFace final
    {
        explicit inline FreeTypeFace(FT_Face face) noexcept : face_{face}
        {
        }

      public:
        inline FreeTypeFace(const FreeTypeFace &other) noexcept : face_(ref_count(other.face_.get()))
        {
        }
        FreeTypeFace(FreeTypeFace &&) noexcept = default;
        ~FreeTypeFace() noexcept = default;

        inline FreeTypeFace &operator=(const FreeTypeFace &other) noexcept
        {
            face_ = ref_count(other.face_.get());
            return *this;
        }
        FreeTypeFace &operator=(FreeTypeFace &&) noexcept = default;

        [[nodiscard]] FT_Face get() const noexcept
        {
            return face_.get();
        }

        [[nodiscard]] std::remove_pointer_t<FT_Face> &operator*() const noexcept
        {
            return *face_;
        }

        [[nodiscard]] std::remove_pointer_t<FT_Face> *operator->() const noexcept
        {
            return face_.get();
        }

      private:
        friend class FreeTypeLibrary;

        using Ptr = std::unique_ptr<std::remove_pointer_t<FT_Face>, FreeTypeFaceDeleter>;

        static inline Ptr ref_count(FT_Face face) noexcept
        {
            if (face == nullptr)
                return Ptr{};
            FT_Reference_Face(face);
            return Ptr{face};
        }

        Ptr face_;
    };

    struct FreeTypeLibraryDeleter final
    {
        inline void operator()(FT_Library library) const noexcept
        {
            FT_Done_FreeType(library);
        }
    };

    class FreeTypeLibrary final
    {
      public:
        FreeTypeLibrary() = default;

        inline FreeTypeLibrary(const FreeTypeLibrary &other) noexcept
            : library_(ref_count_library(other.library_.get()))
        {
        }
        FreeTypeLibrary(FreeTypeLibrary &&) noexcept = default;
        ~FreeTypeLibrary() noexcept = default;

        inline FreeTypeLibrary &operator=(const FreeTypeLibrary &other) noexcept
        {
            library_ = ref_count_library(other.library_.get());
            return *this;
        }
        FreeTypeLibrary &operator=(FreeTypeLibrary &&) noexcept = default;

        [[nodiscard]] inline FT_Library get() const noexcept
        {
            return library_.get();
        }

        [[nodiscard]] inline FreeTypeFace load_face(const std::span<const std::byte> buffer,
                                                    const std::int32_t face_index = 0) const
        {
            FT_Face face;
            throw_freetype_error(FT_New_Memory_Face(library_.get(),
                                                    reinterpret_cast<const FT_Byte *>(buffer.data()),
                                                    static_cast<std::int32_t>(buffer.size()),
                                                    face_index,
                                                    &face),
                                 "Failed to load FreeType face");
            return FreeTypeFace{face};
        }

      private:
        using Ptr = std::unique_ptr<std::remove_pointer_t<FT_Library>, FreeTypeLibraryDeleter>;

        static inline Ptr create_library()
        {
            FT_Library ptr;
            throw_freetype_error(FT_Init_FreeType(&ptr), "Failed to initialize FreeType library");
            return Ptr{ptr};
        }

        static inline Ptr ref_count_library(FT_Library library) noexcept
        {
            if (library == nullptr)
                return Ptr{};
            FT_Reference_Library(library);
            return Ptr{library};
        }

        Ptr library_ = create_library();
    };
} // namespace retro
