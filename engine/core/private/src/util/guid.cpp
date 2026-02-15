/**
 * @file guid.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/uuid.hpp>

module retro.core.util.guid;

namespace retro
{
    Guid Guid::create()
    {
        static boost::uuids::random_generator generator;
        return Guid{generator()};
    }

    Guid Guid::create_v7()
    {
        static boost::uuids::time_generator_v7 generator;
        return Guid{generator()};
    }
} // namespace retro
