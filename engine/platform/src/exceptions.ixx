/**
 * @file exceptions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.platform:exceptions;

import std;

namespace retro
{
    export class PlatformException final : public std::exception
    {
      public:
        explicit inline PlatformException(std::string message) : message_{std::move(message)}
        {
        }

        [[nodiscard]] inline const char *what() const noexcept override
        {
            return message_.c_str();
        }

      private:
        std::string message_;
    };
} // namespace retro
