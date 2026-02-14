/**
 * @file date_time.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.util.date_time;

import std;

namespace retro
{
    export using DateTime = std::chrono::utc_clock::time_point;
    export using TimeSpan = std::chrono::duration<double>;
} // namespace retro
