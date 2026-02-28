/**
 * @file interop_function.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.functional.interop_function;

import std;

namespace retro
{
    template <typename R, typename... Args>
    struct InteropStorage
    {
        using Invoke = R (*)(void *, Args...);
        using Equals = bool (*)(void *, void *);

        Invoke invoke = nullptr;
        std::unique_ptr<void, void (*)(void *)> closure;
        Equals equals = nullptr;
    };

    export template <typename>
    class InteropFunction;

    export template <typename R, typename... Args>
    class InteropFunction<R(Args...)>
    {
        using Storage = InteropStorage<R, Args...>;

      public:
        using InvokePtr = Storage::Invoke;
        using EqualsPtr = Storage::Equals;

        InteropFunction() = default;

        InteropFunction(InvokePtr invoke, std::unique_ptr<void, void (*)(void *)> closure, EqualsPtr equals)
            : storage_{std::make_shared<Storage>(invoke, std::move(closure), equals)}
        {
        }

        R operator()(Args... args) const
        {
            if (storage_ == nullptr)
            {
                throw std::bad_function_call{};
            }

            return storage_->invoke(storage_->closure.get(), std::forward<Args>(args)...);
        }

        [[nodiscard]] friend bool operator==(const InteropFunction &lhs, const InteropFunction &rhs)
        {
            if (lhs.storage_ == nullptr || rhs.storage_ == nullptr) [[unlikely]]
            {
                return lhs.storage_ == rhs.storage_;
            }

            return lhs.storage_->equals(lhs.storage_->closure.get(), rhs.storage_->closure.get());
        }

      private:
        std::shared_ptr<Storage> storage_;
    };
} // namespace retro
