/**
 * @file service_call_site.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_call_site;

import std;
import :service_instance;

namespace retro
{
    using SingletonFactory = Delegate<ServiceInstance(class ServiceProvider &)>;
    using TransientFactory = Delegate<void *(ServiceProvider &)>;

    struct RealizedSingleton
    {
        std::size_t instance_index = static_cast<std::size_t>(-1);
    };

    struct UnrealizedSingleton
    {
        SingletonFactory registration{};
    };

    struct DirectTransient
    {
    };

    struct DerivedTransient
    {
        TransientFactory registration{};
    };

    using ServiceCallSite = std::variant<UnrealizedSingleton, RealizedSingleton, DirectTransient, DerivedTransient>;
} // namespace retro
