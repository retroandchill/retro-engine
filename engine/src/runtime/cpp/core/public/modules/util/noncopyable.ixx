/**
 * @file noncopyable.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.util.noncopyable;

namespace retro
{
    /**
     * Mixin class to define a simple type that is not able to be copied or moved.
     */
    export class NonCopyable
    {
      protected:
        /**
         * Default constructor, marked as protected to prevent direct instantiation.
         */
        NonCopyable() = default;

        /**
         * Default no-op destructor, marked protected to discourage managing a pointer to this type.
         */
        ~NonCopyable() = default;

      public:
        NonCopyable(const NonCopyable &) = delete;
        NonCopyable(NonCopyable &&) = delete;

        NonCopyable &operator=(const NonCopyable &) = delete;
        NonCopyable &operator=(NonCopyable &&) = delete;
    };
} // namespace retro
