/**
 * @file exceptions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime.exceptions;

import std;
import retro.core.util.exceptions;

namespace retro
{
    export class GraphicsException : public std::runtime_error
    {
      public:
        using std::runtime_error::runtime_error;
    };
} // namespace retro
