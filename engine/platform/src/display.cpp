/**
 * @file display.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.platform;

namespace retro
{
    std::unique_ptr<Window> create_sdl_window(const WindowDesc &desc);
    std::shared_ptr<Window> create_shared_sdl_window(const WindowDesc &desc);

    std::unique_ptr<Window> Window::create(const WindowDesc &desc)
    {
        switch (desc.backend)
        {
            case WindowBackend::SDL3:
                return create_sdl_window(desc);
            default:
                throw std::invalid_argument{"Unsupported window backend"};
        }
    }

    std::shared_ptr<Window> Window::create_shared(const WindowDesc &desc)
    {
        switch (desc.backend)
        {
            case WindowBackend::SDL3:
                return create_shared_sdl_window(desc);
            default:
                throw std::invalid_argument{"Unsupported window backend"};
        }
    }
} // namespace retro
