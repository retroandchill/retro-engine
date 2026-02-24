/**
 * @file service_provider.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.di:service_provider;

import std;

import retro.core.algorithm.hashing;
import retro.core.type_traits.pointer;
import retro.core.type_traits.range;
import retro.core.functional.overload;
import :metadata;
import :service_instance;
import :service_call_site;
import :service_identifier;
import retro.core.functional.delegate;
import retro.core.containers.optional;

namespace retro
{
    export class RETRO_API ServiceNotFoundException : public std::exception
    {
      public:
        [[nodiscard]] const char *what() const noexcept override;
    };

    export class ServiceProvider;

    template <typename T>
    concept ServiceCompatibleContainer =
        std::ranges::range<T> && ContainerAppendable<T, PointerElement<std::ranges::range_reference_t<T>>> &&
        std::is_pointer_v<std::ranges::range_value_t<T>>;

    class ServiceProvider
    {
      public:
        virtual ~ServiceProvider() = default;

        template <typename T>
        auto get()
        {
            if constexpr (ServiceCompatibleContainer<std::decay_t<T>>)
            {
                using DecayedT = std::decay_t<T>;
                using ElementType = PointerElementT<std::ranges::range_reference_t<DecayedT>>;
                return get_all(typeid(ElementType)) |
                       std::views::transform([](void *value) { return static_cast<ElementType *>(value); }) |
                       std::ranges::to<DecayedT>();
            }
            else if constexpr (HandleWrapper<T>)
            {
                using PtrType = HandleType<T>;
                PtrType ptr{static_cast<PtrType>(get_raw(typeid(T)))};
                if (ptr == nullptr)
                {
                    return Optional<PtrType>{};
                }

                return Optional<PtrType>{std::move(ptr)};
            }
            else
            {
                return Optional<T &>{static_cast<T *>(get_raw(typeid(T)))};
            }
        }

        template <typename T>
        decltype(auto) get_required()
        {
            if constexpr (ServiceCompatibleContainer<std::decay_t<T>>)
            {
                return get<T>();
            }
            else
            {
                return validate_service(get<T>());
            }
        }

        virtual void *get_raw(const std::type_info &type) = 0;
        virtual std::generator<void *> get_all(const std::type_info &type) = 0;

      private:
        template <typename T>
        T validate_service(Optional<T> opt)
        {
            if (!opt.has_value())
            {
                throw ServiceNotFoundException{};
            }

            return *std::move(opt);
        }
    };

} // namespace retro
