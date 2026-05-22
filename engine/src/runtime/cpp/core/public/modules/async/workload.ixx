/**
 * @file workload.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.async.workload;

import std;
import retro.core.async.task;

namespace retro
{
    export class RETRO_API Workload
    {
      public:
        Workload() = default;

        inline Workload(std::function<bool(std::size_t, std::size_t)> worker_function, const std::size_t chunks)
            : worker_function_{std::move(worker_function)}, chunks_{chunks}
        {
        }

        [[nodiscard]] bool finish(std::int32_t thread_count) const;

        [[nodiscard]] Task<bool> finish_async(std::int32_t thread_count) const;

      private:
        [[nodiscard]] bool finish_sequential() const;
        [[nodiscard]] Task<bool> finish_parallel(std::int32_t thread_count) const;

        std::function<bool(std::size_t, std::size_t)> worker_function_;
        std::size_t chunks_{0};
    };
} // namespace retro
