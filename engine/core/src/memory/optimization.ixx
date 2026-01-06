/**
 * @file optimization.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#ifdef _MSC_VER
#include <windows.h>
#endif

export module retro.core:memory.optimization;

namespace retro
{
    export inline void prefetch(const void *addr)
    {
#ifdef _MSC_VER
        // Windows / MSVC
        _mm_prefetch(static_cast<const char *>(addr), _MM_HINT_T0);
#elif defined(__GNUC__) || defined(__clang__)
        // GCC / Clang (Linux, macOS, Android, etc.)
        // 0 = read-only, 3 = high temporal locality (keep in all cache levels)
        __builtin_prefetch(addr, 0, 3);
#else
        // Fallback for unknown compilers
        (void)addr;
#endif
    }
} // namespace retro
