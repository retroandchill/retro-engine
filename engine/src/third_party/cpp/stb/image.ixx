/**
 * @file image.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#ifdef _DEBUG
#define STBI_FAILURE_USERMSG
#endif
#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>

export module stb.image;

import std;

namespace stb::image
{
    export enum class ComponentType
    {
        u8,
        u16,
        f32,
    };

    export [[nodiscard]] constexpr std::size_t component_size(const ComponentType component_type)
    {
        switch (component_type)
        {
            case ComponentType::u8:
                return sizeof(stbi_uc);
            case ComponentType::u16:
                return sizeof(stbi_us);
            case ComponentType::f32:
                return sizeof(float);
            default:
                return 0;
        }
    }

    export enum class Channels
    {
        use_desired = STBI_default,
        grey = STBI_grey,
        grey_alpha = STBI_grey_alpha,
        rgb = STBI_rgb,
        rgb_alpha = STBI_rgb_alpha,
    };

    export [[nodiscard]] constexpr std::size_t channel_count(const Channels channels)
    {
        switch (channels)
        {
            case Channels::grey:
                return 1;
            case Channels::grey_alpha:
                return 2;
            case Channels::rgb:
                return 3;
            case Channels::rgb_alpha:
                return 4;
            case Channels::use_desired:
            default:
                return 0;
        }
    }

    export struct Extent2D
    {
        std::int32_t width{};
        std::int32_t height{};
    };

    export struct ImageInfo
    {
        Extent2D extent{};
        Channels channels{};
    };

    export class Error final : public std::exception
    {
      public:
        [[nodiscard]] constexpr const char *what() const noexcept override
        {
            return message_;
        }

      private:
        const char *message_{stbi_failure_reason()};
    };

    export template <typename T>
    concept IoReadable = std::derived_from<T, std::istream> || requires(T &readable, char *data, int size) {
        {
            readable.read(std::span{data, static_cast<std::size_t>(size)})
        } -> std::convertible_to<int>;
        {
            readable.skip(size)
        };
        {
            readable.eof()
        } -> std::convertible_to<bool>;
    };

    template <IoReadable T>
    struct CallbackContext
    {
        T *readable{};
        std::exception_ptr exception{};
    };

    template <IoReadable T>
    constexpr stbi_io_callbacks callbacks{
        .read = [](void *context, char *data, const int size) -> int
        {
            auto &s = *static_cast<CallbackContext<T> *>(context);
            try
            {
                return static_cast<int>(s.readable->read(std::span{data, static_cast<std::size_t>(size)}));
            }
            catch (...)
            {
                s.exception = std::current_exception();
                return 0;
            }
        },
        .skip = [](void *context, const int n) -> void
        {
            auto &s = *static_cast<CallbackContext<T> *>(context);
            try
            {
                s.readable->skip(n);
            }
            catch (...)
            {
                s.exception = std::current_exception();
            }
        },
        .eof = [](void *context) -> int
        {
            auto &s = *static_cast<CallbackContext<T> *>(context);
            try
            {
                return s.readable->eof();
            }
            catch (...)
            {
                s.exception = std::current_exception();
                return 0;
            }
        }};

    template <>
    constexpr stbi_io_callbacks callbacks<std::istream>{
        .read = [](void *context, char *data, const int size) -> int
        {
            auto &s = *static_cast<CallbackContext<std::istream> *>(context);
            try
            {
                return static_cast<int>(s.readable->readsome(data, size));
            }
            catch (...)
            {
                s.exception = std::current_exception();
                return 0;
            }
        },
        .skip = [](void *context, const int n) -> void
        {
            auto &s = *static_cast<CallbackContext<std::istream> *>(context);
            try
            {
                s.readable->seekg(n, std::ios_base::cur);
            }
            catch (...)
            {
                s.exception = std::current_exception();
            }
        },
        .eof = [](void *context) -> int
        {
            auto &s = *static_cast<CallbackContext<std::istream> *>(context);
            try
            {
                return s.readable->eof();
            }
            catch (...)
            {
                s.exception = std::current_exception();
                return 0;
            }
        }};

    export template <typename T>
    concept CStringConvertible = requires(T &&t) {
        {
            t.c_str()
        } -> std::convertible_to<const char *>;
    };

    struct ImageDeleter
    {
        inline void operator()(void *ptr) const noexcept
        {
            stbi_image_free(ptr);
        }
    };

    template <typename T>
    using DataPtr = std::unique_ptr<T, ImageDeleter>;

    export class Image
    {
      public:
        constexpr Image() = default;

      private:
        constexpr Image(std::byte *data,
                        std::int32_t width,
                        std::int32_t height,
                        Channels channels,
                        ComponentType component)
            : data_{data}, width_{width}, height_{height}, channels_{channels}, component_type_{component}
        {
        }

      public:
        Image(const Image &) = delete;
        constexpr Image(Image &&other) noexcept
            : data_{std::move(other.data_)}, width_{std::exchange(other.width_, 0)},
              height_{std::exchange(other.height_, 0)},
              channels_{std::exchange(other.channels_, Channels::use_desired)},
              component_type_{std::exchange(other.component_type_, ComponentType::u8)}
        {
        }

        constexpr ~Image() = default;

        Image &operator=(const Image &) = delete;
        constexpr Image &operator=(Image &&other) noexcept
        {
            if (this != &other)
            {
                data_ = std::move(other.data_);
                width_ = std::exchange(other.width_, 0);
                height_ = std::exchange(other.height_, 0);
                channels_ = std::exchange(other.channels_, Channels::use_desired);
                component_type_ = std::exchange(other.component_type_, ComponentType::u8);
            }

            return *this;
        }

        [[nodiscard]] constexpr std::byte *data() noexcept
        {
            return data_.get();
        }

        [[nodiscard]] constexpr const std::byte *data() const noexcept
        {
            return data_.get();
        }

        [[nodiscard]] constexpr std::span<std::byte> bytes() noexcept
        {
            return {data_.get(), byte_size()};
        }

        [[nodiscard]] constexpr std::span<const std::byte> bytes() const noexcept
        {
            return {data_.get(), byte_size()};
        }

        [[nodiscard]] constexpr std::int32_t width() const noexcept
        {
            return width_;
        }

        [[nodiscard]] constexpr std::int32_t height() const noexcept
        {
            return height_;
        }

        [[nodiscard]] constexpr Channels channels() const noexcept
        {
            return channels_;
        }

        [[nodiscard]] constexpr ComponentType component_type() const noexcept
        {
            return component_type_;
        }

        static inline Image load(std::span<const std::byte> data, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_from_memory, ComponentType::u8>(data, desired_channels);
        }

        template <IoReadable T>
        static inline Image load(T &stream, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_from_callbacks, ComponentType::u8>(stream, desired_channels);
        }

#ifndef STBI_NO_STDIO
        static inline Image load(const char *filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load, ComponentType::u8>(filename, desired_channels);
        }

        template <CStringConvertible T>
        static inline Image load(const T &filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load, ComponentType::u8>(filename, desired_channels);
        }

        static inline Image load(const std::filesystem::path &filename,
                                 Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load, ComponentType::u8>(filename, desired_channels);
        }

        static inline Image load(std::FILE &file, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_from_file, ComponentType::u8>(file, desired_channels);
        }
#endif

        static inline Image load_u16(std::span<const std::byte> data, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_16_from_memory, ComponentType::u16>(data, desired_channels);
        }

        template <IoReadable T>
        static inline Image load_u16(T &stream, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_16_from_callbacks, ComponentType::u16>(stream, desired_channels);
        }

#ifndef STBI_NO_STDIO
        static inline Image load_u16(const char *filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_16, ComponentType::u16>(filename, desired_channels);
        }

        template <CStringConvertible T>
        static inline Image load_u16(const T &filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_16, ComponentType::u16>(filename, desired_channels);
        }

        static inline Image load_u16(const std::filesystem::path &filename,
                                     Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_16, ComponentType::u16>(filename, desired_channels);
        }

        static inline Image load_u16(std::FILE &file, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_load_from_file_16, ComponentType::u16>(file, desired_channels);
        }
#endif

#ifndef STBI_NO_LINEAR
        static inline Image load_f32(std::span<const std::byte> data, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf_from_memory, ComponentType::f32>(data, desired_channels);
        }

        template <IoReadable T>
        static inline Image load_f32(T &stream, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf_from_callbacks, ComponentType::f32>(stream, desired_channels);
        }

#ifndef STBI_NO_STDIO
        static inline Image load_f32(const char *filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf, ComponentType::f32>(filename, desired_channels);
        }

        template <CStringConvertible T>
        static inline Image load_f32(const T &filename, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf, ComponentType::f32>(filename, desired_channels);
        }

        static inline Image load_f32(const std::filesystem::path &filename,
                                     Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf, ComponentType::f32>(filename, desired_channels);
        }

        static inline Image load_f32(std::FILE &file, Channels desired_channels = Channels::use_desired)
        {
            return load<stbi_loadf_from_file, ComponentType::f32>(file, desired_channels);
        }
#endif
#endif

      private:
        template <auto Functor, ComponentType Type>
            requires std::invocable<decltype(Functor), const stbi_uc *, int, int *, int *, int *, int>
        static inline Image load(std::span<const std::byte> data, Channels desired_channels = Channels::use_desired)
        {
            std::int32_t width;
            std::int32_t height;
            std::int32_t channels;
            auto *buffer = std::invoke(Functor,
                                       reinterpret_cast<const stbi_uc *>(data.data()),
                                       static_cast<std::int32_t>(data.size()),
                                       &width,
                                       &height,
                                       &channels,
                                       static_cast<std::int32_t>(desired_channels));
            if (buffer == nullptr)
                throw Error{};

            if (desired_channels != Channels::use_desired)
            {
                channels = static_cast<std::int32_t>(desired_channels);
            }

            return Image{std::launder(reinterpret_cast<std::byte *>(buffer)),
                         width,
                         height,
                         static_cast<Channels>(channels),
                         Type};
        }

        template <auto Functor, ComponentType Type, IoReadable T>
            requires std::invocable<decltype(Functor), const stbi_io_callbacks *, void *, int *, int *, int *, int>
        static inline Image load(T &stream, Channels desired_channels = Channels::use_desired)
        {
            CallbackContext context{std::addressof(stream), nullptr};
            std::int32_t width;
            std::int32_t height;
            std::int32_t channels;
            auto *buffer = std::invoke(Functor,
                                       &callbacks<T>,
                                       &context,
                                       &width,
                                       &height,
                                       &channels,
                                       static_cast<std::int32_t>(desired_channels));
            if (context.exception)
                std::rethrow_exception(context.exception);

            if (buffer == nullptr)
                throw Error{};

            return Image{std::launder(reinterpret_cast<std::byte *>(buffer)),
                         width,
                         height,
                         static_cast<Channels>(channels),
                         Type};
        }

#ifndef STBI_NO_STDIO
        template <auto Functor, ComponentType Type>
            requires std::invocable<decltype(Functor), const char *, int *, int *, int *, int>
        static inline Image load(const char *filename, Channels desired_channels = Channels::use_desired)
        {
            std::int32_t width;
            std::int32_t height;
            std::int32_t channels;
            auto *buffer =
                std::invoke(Functor, filename, &width, &height, &channels, static_cast<std::int32_t>(desired_channels));
            if (buffer == nullptr)
                throw Error{};

            return Image{std::launder(reinterpret_cast<std::byte *>(buffer)),
                         width,
                         height,
                         static_cast<Channels>(channels),
                         Type};
        }

        template <auto Functor, ComponentType Type, CStringConvertible T>
            requires std::invocable<decltype(Functor), const char *, int *, int *, int *, int>
        static inline Image load(const T &filename, Channels desired_channels = Channels::use_desired)
        {
            return load<Functor, Type>(filename.c_str(), desired_channels);
        }

        template <auto Functor, ComponentType Type>
            requires std::invocable<decltype(Functor), const char *, int *, int *, int *, int>
        static inline Image load(const std::filesystem::path &filename,
                                 Channels desired_channels = Channels::use_desired)
        {
#ifdef _WIN32
            return load<Functor, Type>(filename.string(), desired_channels);
#else
            return load<Functor, Type>(filename.c_str(), desired_channels);
#endif
        }

        template <auto Functor, ComponentType Type>
            requires std::invocable<decltype(Functor), std::FILE *, int *, int *, int *, int>
        static inline Image load(std::FILE &file, Channels desired_channels = Channels::use_desired)
        {
            std::int32_t width;
            std::int32_t height;
            std::int32_t channels;
            auto *buffer =
                std::invoke(Functor, &file, &width, &height, &channels, static_cast<std::int32_t>(desired_channels));
            if (buffer == nullptr)
                throw Error{};

            return Image{std::launder(reinterpret_cast<std::byte *>(buffer)),
                         width,
                         height,
                         static_cast<Channels>(channels),
                         Type};
        }
#endif

        [[nodiscard]] std::size_t element_count() const noexcept
        {
            return static_cast<std::size_t>(width_) * static_cast<std::size_t>(height_) * channel_count(channels_);
        }

        [[nodiscard]] std::size_t byte_size() const noexcept
        {
            return element_count() * component_size(component_type_);
        }

        DataPtr<std::byte[]> data_{};
        std::int32_t width_{};
        std::int32_t height_{};
        Channels channels_{};
        ComponentType component_type_{};
    };

#ifndef STBI_NO_GIF
    export class Gif
    {
      public:
        constexpr Gif() = default;

      private:
        constexpr Gif(std::byte *data,
                      std::int32_t *delays,
                      std::int32_t width,
                      std::int32_t height,
                      std::int32_t num_frames,
                      Channels channels)
            : data_{data}, delays_{delays}, width_{width}, height_{height}, num_frames_{num_frames}, channels_{channels}
        {
        }

      public:
        Gif(const Gif &) = delete;
        constexpr Gif(Gif &&other) noexcept
            : data_{std::move(other.data_)}, delays_{std::move(other.delays_)}, width_{std::exchange(other.width_, 0)},
              height_{std::exchange(other.height_, 0)}, num_frames_{std::exchange(other.num_frames_, 0)},
              channels_{std::exchange(other.channels_, Channels::use_desired)}
        {
        }

        constexpr ~Gif() noexcept = default;

        Gif &operator=(const Gif &) = delete;

        constexpr Gif &operator=(Gif &&other) noexcept
        {
            if (this != &other)
            {
                data_ = std::move(other.data_);
                delays_ = std::move(other.delays_);
                width_ = std::exchange(other.width_, 0);
                height_ = std::exchange(other.height_, 0);
                num_frames_ = std::exchange(other.num_frames_, 0);
            }

            return *this;
        }

        [[nodiscard]] constexpr std::byte *data() noexcept
        {
            return data_.get();
        }

        [[nodiscard]] constexpr const std::byte *data() const noexcept
        {
            return data_.get();
        }

        [[nodiscard]] constexpr std::span<std::byte> bytes() noexcept
        {
            return {data_.get(), element_count()};
        }

        [[nodiscard]] constexpr std::span<const std::byte> bytes() const noexcept
        {
            return {data_.get(), element_count()};
        }

        [[nodiscard]] constexpr std::span<std::int32_t> delays() noexcept
        {
            return {delays_.get(), static_cast<std::size_t>(num_frames_)};
        }

        [[nodiscard]] constexpr std::span<const std::int32_t> delays() const noexcept
        {
            return {delays_.get(), static_cast<std::size_t>(num_frames_)};
        }

        [[nodiscard]] constexpr std::int32_t width() const noexcept
        {
            return width_;
        }

        [[nodiscard]] constexpr std::int32_t height() const noexcept
        {
            return height_;
        }

        [[nodiscard]] constexpr std::int32_t num_frames() const noexcept
        {
            return num_frames_;
        }

        [[nodiscard]] constexpr Channels channels() const noexcept
        {
            return channels_;
        }

      private:
        [[nodiscard]] std::size_t element_count() const noexcept
        {
            return static_cast<std::size_t>(width_) * static_cast<std::size_t>(height_) *
                   static_cast<std::size_t>(num_frames_) * channel_count(channels_);
        }

        DataPtr<std::byte[]> data_{};
        DataPtr<std::int32_t[]> delays_{};
        std::int32_t width_{};
        std::int32_t height_{};
        std::int32_t num_frames_{};
        Channels channels_{};
    };
#endif

#ifndef STBI_NO_HDR
    export inline void hdr_to_ldr_gamma(const float gamma)
    {
        stbi_hdr_to_ldr_gamma(gamma);
    }

    export inline void hdr_to_ldr_scale(const float scale)
    {
        stbi_hdr_to_ldr_scale(scale);
    }
#endif

#ifndef STBI_NO_LINEAR
    export inline void ldr_to_hdr_gamma(const float gamma)
    {
        stbi_ldr_to_hdr_gamma(gamma);
    }
    export inline void ldr_to_hdr_scale(const float scale)
    {
        stbi_ldr_to_hdr_scale(scale);
    }
#endif // STBI_NO_LINEAR

    export inline bool is_hdr(std::span<const std::byte> data)
    {
        return stbi_is_hdr_from_memory(reinterpret_cast<const stbi_uc *>(data.data()),
                                       static_cast<std::int32_t>(data.size()));
    }

    export template <IoReadable T>
    bool is_hdr(T &stream)
    {
        CallbackContext context{std::addressof(stream), nullptr};
        return stbi_is_hdr_from_callbacks(&callbacks<std::istream>, &context);
    }

#ifndef STBI_NO_STDIO
    export inline bool is_hdr(const char *filename)
    {
        return stbi_is_hdr(filename);
    }

    export template <CStringConvertible T>
    bool is_hdr(const T &filename)
    {
        return stbi_is_hdr(filename.c_str());
    }

    export inline bool is_hdr(const std::filesystem::path &filename)
    {
#ifdef _WIN32
        return is_hdr(filename.string());
#else
        return is_hdr(filename.c_str());
#endif
    }

    export inline bool is_hdr(std::FILE &file)
    {
        return stbi_is_hdr_from_file(&file);
    }
#endif

    export inline ImageInfo info(std::span<const std::byte> data)
    {
        int width;
        int height;
        int comp;
        if (!stbi_info_from_memory(reinterpret_cast<const stbi_uc *>(data.data()),
                                   static_cast<std::int32_t>(data.size()),
                                   &width,
                                   &height,
                                   &comp))
        {
            throw Error{};
        }

        return ImageInfo{.extent = {width, height}, .channels = static_cast<Channels>(comp)};
    }

    export template <IoReadable T>
    ImageInfo info(T &stream)
    {
        CallbackContext context{std::addressof(stream), nullptr};
        int width;
        int height;
        int comp;

        auto result = stbi_info_from_callbacks(&callbacks<std::istream>, &context, &width, &height, &comp);
        if (context.exception)
        {
            std::rethrow_exception(context.exception);
        }

        if (!result)
        {
            throw Error{};
        }

        return ImageInfo{.extent = {width, height}, .channels = static_cast<Channels>(comp)};
    }

#ifndef STBI_NO_STDIO
    export inline ImageInfo info(const char *filename)
    {
        int width;
        int height;
        int comp;
        if (!stbi_info(filename, &width, &height, &comp))
        {
            throw Error{};
        }

        return ImageInfo{.extent = {width, height}, .channels = static_cast<Channels>(comp)};
    }

    export template <CStringConvertible T>
    ImageInfo info(const T &filename)
    {
        return info(filename.c_str());
    }

    export inline ImageInfo info(const std::filesystem::path &filename)
    {
#ifdef _WIN32
        return info(filename.string());
#else
        return info(filename.c_str());
#endif
    }

    export inline ImageInfo info(std::FILE &file)
    {
        int width;
        int height;
        int comp;
        if (!stbi_info_from_file(&file, &width, &height, &comp))
        {
            throw Error{};
        }

        return ImageInfo{.extent = {width, height}, .channels = static_cast<Channels>(comp)};
    }
#endif

    export bool is_16_bit(const std::span<const std::byte> buffer)
    {
        return stbi_is_16_bit_from_memory(reinterpret_cast<const stbi_uc *>(buffer.data()),
                                          static_cast<std::int32_t>(buffer.size())) != 0;
    }

    export template <IoReadable T>
    bool is_16_bit(T &stream)
    {
        CallbackContext context{std::addressof(stream), nullptr};
        auto result = stbi_is_16_bit_from_callbacks(&callbacks<std::istream>, &context);
        if (context.exception)
        {
            std::rethrow_exception(context.exception);
        }

        return result != 0;
    }

#ifndef STBI_NO_STDIO
    export bool is_16_bit(const char *filename)
    {
        return stbi_is_16_bit(filename) != 0;
    }

    export template <CStringConvertible T>
    bool is_16_bit(const T &filename)
    {
        return is_16_bit(filename.c_str());
    }

    export inline bool is_16_bit(const std::filesystem::path &filename)
    {
#if _WIN32
        return is_16_bit(filename.string());
#else
        return is_16_bit(filename.c_str());
#endif
    }
#endif

    export inline void set_unpremultiply_on_load(const bool should_unpremultiply)
    {
        stbi_set_unpremultiply_on_load(should_unpremultiply);
    }

    export inline void convert_iphone_png_to_rgb(const bool should_convert)
    {
        stbi_convert_iphone_png_to_rgb(should_convert);
    }

    export inline void set_flip_vertically_on_load(const bool should_flip)
    {
        stbi_set_flip_vertically_on_load(should_flip);
    }

    export inline void set_unpremultiply_on_load_thread(const bool should_unpremultiply)
    {
        stbi_set_unpremultiply_on_load_thread(should_unpremultiply);
    }

    export inline void convert_iphone_png_to_rgb_thread(const bool should_convert)
    {
        stbi_convert_iphone_png_to_rgb_thread(should_convert);
    }

    export inline void set_flip_vertically_on_load_thread(const bool should_flip)
    {
        stbi_set_flip_vertically_on_load_thread(should_flip);
    }
} // namespace stb::image
