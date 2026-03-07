/**
 * @file spsc_circular_queue.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.containers.spsc_circular_queue;

import std;

namespace retro
{
    template <typename T>
    concept Resetable = std::is_default_constructible_v<T> && requires(T &t) {
        t.reset();
        requires noexcept(t.reset());
    };

    export template <Resetable T, std::size_t Capacity>
    class SpscCircularQueue
    {
      public:
        template <std::invocable<T &> Functor>
        void produce(Functor &&functor)
        {
            free_slots_.acquire();

            auto &slot = slots_[write_index_];
            try
            {
                std::invoke(std::forward<Functor>(functor), slot);
            }
            catch (...)
            {
                slot.reset();
                free_slots_.release();
                throw;
            }

            write_index_ = (write_index_ + 1) % Capacity;
            ready_slots_.release();
        }

        template <std::invocable<T &> Functor>
        void consume(Functor &&functor)
        {
            ready_slots_.acquire();

            auto &slot = slots_[read_index_];
            try
            {
                std::invoke(std::forward<Functor>(functor), slot);
            }
            catch (...)
            {
                ready_slots_.release();
                throw;
            }

            slot.reset();
            read_index_ = (read_index_ + 1) % Capacity;
            free_slots_.release();
        }

      private:
        std::array<T, Capacity> slots_;
        std::size_t write_index_ = 0;
        std::size_t read_index_ = 0;

        std::counting_semaphore<Capacity> free_slots_{Capacity};
        std::counting_semaphore<Capacity> ready_slots_{0};
    };
} // namespace retro
