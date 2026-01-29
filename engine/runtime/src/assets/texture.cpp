/**
 * @file texture.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    namespace
    {
        constexpr std::array PNG_HEADER = {std::byte{0x89},
                                           std::byte{0x50},
                                           std::byte{0x4E},
                                           std::byte{0x47},
                                           std::byte{0x0D},
                                           std::byte{0x0A},
                                           std::byte{0x1A},
                                           std::byte{0x0A}};
    }

    bool TextureDecoder::can_decode(const AssetDecodeContext &context, BufferedStream &stream) const
    {
        // TODO: For now we'll just support PNG images, but we may want to open it up to others later
        auto peek_result = stream.peek(PNG_HEADER.size());
        if (!peek_result.has_value())
        {
            return false;
        }

        return std::ranges::equal(*peek_result, PNG_HEADER);
    }

    AssetLoadResult<RefCountPtr<Asset>> TextureDecoder::decode(const AssetDecodeContext &context,
                                                               BufferedStream &stream)
    {
        throw NotImplementedException{};
    }
} // namespace retro
