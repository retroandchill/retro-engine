/**
 * @file interop_error.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.interop.interop_error;

import std;

namespace retro
{
    namespace
    {
        thread_local std::string last_error_message;
    }

    const char *cache_error_message(const char *message)
    {
        last_error_message = message;
        return last_error_message.data();
    }
} // namespace retro
