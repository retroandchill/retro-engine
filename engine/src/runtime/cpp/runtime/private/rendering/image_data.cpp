/**
 * @file image_data.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime.rendering.image_data;

import retro.core.util.exceptions;

namespace retro
{
    ImageData ImageData::create_from_memory(const std::span<const std::byte> bytes) noexcept
    {
        return ImageData{stb::image::Image::load(bytes, stb::image::Channels::rgb_alpha)};
    }
} // namespace retro
