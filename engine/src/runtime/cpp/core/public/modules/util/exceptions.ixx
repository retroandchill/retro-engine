/**
 * @file exceptions.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.util.exceptions;

import std;

namespace retro
{
    export class IoException : public std::runtime_error
    {
      public:
        using std::runtime_error::runtime_error;
    };

    export class ResourceException : public std::runtime_error
    {
      public:
        using std::runtime_error::runtime_error;
    };

    export class InvalidStateException final : public std::logic_error
    {
      public:
        using std::logic_error::logic_error;
    };

    export class UnsupportedOperationException final : public std::logic_error
    {
      public:
        using std::logic_error::logic_error;
    };

    export class NotImplementedException final : public std::exception
    {
      public:
        [[nodiscard]] inline const char *what() const noexcept override
        {
            return "This function is not yet implemented, please fix that before release";
        }
    };
} // namespace retro
