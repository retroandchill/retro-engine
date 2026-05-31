/**
 * @file window.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform.headless.window;

import std;
import retro.core.math.vector;
import retro.core.strings.cstring_view;
import retro.platform.window;

namespace retro
{
    export class HeadlessWindow final : public Window
    {
      public:
        explicit HeadlessWindow(const WindowDesc &desc);

        explicit HeadlessWindow(NativeWindowHandle handle);

        [[nodiscard]] std::uint64_t id() const noexcept override;

        [[nodiscard]] PlatformWindowHandle platform_handle() const noexcept override;

        void set_title(CStringView title) override;

        [[nodiscard]] Vector2u size() const override;

      private:
        std::uint64_t id_{};
        Vector2u size_;
    };
} // namespace retro
