/**
 * @file window.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.sdl.window;

import std;
import sdl;
import retro.core.math.vector;
import retro.core.strings.cstring_view;
import retro.platform.window;

namespace retro
{
    export class Sdl3Window final : public Window
    {
      public:
        explicit Sdl3Window(const WindowDesc &desc);

        explicit Sdl3Window(NativeWindowHandle handle);

      private:
        explicit Sdl3Window(SDL::Window window);

      public:
        Sdl3Window(const Sdl3Window &) = delete;
        Sdl3Window(Sdl3Window &&) noexcept = delete;

        ~Sdl3Window() noexcept override;

        Sdl3Window &operator=(const Sdl3Window &) = delete;
        Sdl3Window &operator=(Sdl3Window &&) noexcept = delete;

        [[nodiscard]] std::uint64_t id() const noexcept override;

        [[nodiscard]] PlatformWindowHandle platform_handle() const noexcept override;

        void set_title(CStringView title) override;

        [[nodiscard]] Vector2u size() const override;

      private:
        SDL::Window window_{};
    };
} // namespace retro
