/**
 * @file exceptions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core:exceptions;

import std;

namespace retro
{
    export class NotImplementedException final : std::exception
    {
      public:
        [[nodiscard]] inline const char *what() const noexcept override
        {
            return "This function is not yet implemented, please fix that before release";
        }
    };
} // namespace retro
