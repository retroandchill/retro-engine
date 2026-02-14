/**
 * @file lazy_singleton.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <cassert>

export module retro.core.util.lazy_singleton;

import std;
import retro.core.containers.optional;

namespace retro
{
    export template <typename T>
    class LazySingleton final
    {
      public:
        LazySingleton(const LazySingleton &) = delete;
        LazySingleton(LazySingleton &&) = delete;

        LazySingleton &operator=(const LazySingleton &) = delete;
        LazySingleton &operator=(LazySingleton &&) = delete;

        static T &get()
        {
            return get_lazy(construct).get_value();
        }

        static void tear_down()
        {
            get_lazy(nullptr).reset();
        }

        static Optional<T &> try_get()
        {
            return get_lazy(construct).try_get_value();
        }

      private:
        static LazySingleton &get_lazy(void (*constructor)(void *))
        {
            static LazySingleton instance{constructor};
            return instance;
        }

        alignas(T) std::byte data_[sizeof(T)];
        T *ptr_ = nullptr;

        LazySingleton(void (*constructor)(T *))
        {
            if (constructor != nullptr)
            {
                constructor(std::launder(reinterpret_cast<T *>(data_)));
                ptr_ = data_;
            }
        }

        ~LazySingleton()
        {
            reset();
        }

        static void construct(T *ptr)
        {
            std::construct_at(ptr);
        }

        static void destruct(T *ptr)
        {
            std::destroy_at(ptr);
        }

        T *try_get_value()
        {
            return ptr_;
        }

        T &get_value()
        {
            assert(ptr_ != nullptr);
            return *ptr_;
        }

        void reset()
        {
            if (ptr_ != nullptr)
            {
                destruct(ptr_);
            }
        }
    };
} // namespace retro
