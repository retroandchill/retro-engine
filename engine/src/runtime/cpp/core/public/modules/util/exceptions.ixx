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
        [[nodiscard]] constexpr const char *what() const noexcept override
        {
            return "This function is not yet implemented, please fix that before release";
        }
    };

    export class OperationCancelledException final : public std::exception
    {
      public:
        explicit inline OperationCancelledException(std::stop_token stop_token = {})
            : stop_token_{std::move(stop_token)}
        {
        }

        [[nodiscard]] constexpr const char *what() const noexcept override
        {
            return "Operation cancelled";
        }

        [[nodiscard]] inline const std::stop_token &stop_token() const noexcept
        {
            return stop_token_;
        }

      private:
        std::stop_token stop_token_;
    };

    export inline void throw_if_stop_requested(const std::stop_token &stop_token)
    {
        if (stop_token.stop_requested())
            throw OperationCancelledException{stop_token};
    }

    export class PlatformException final : public std::runtime_error
    {
      public:
        using std::runtime_error::runtime_error;
    };

    export class GraphicsException : public std::runtime_error
    {
      public:
        using std::runtime_error::runtime_error;
    };
} // namespace retro
