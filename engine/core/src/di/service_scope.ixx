/**
 * @file service_scope.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.core.di:service_scope;

import :service_provider;
import :service_collection;
import retro.core.strings.name;
import retro.core.functional.delegate;

namespace retro
{
    export class ServiceScope
    {
      public:
        virtual ~ServiceScope() = default;

        virtual Name &name() = 0;

        virtual ServiceProvider &service_provider() = 0;
    };

    export class ServiceScopeFactory
    {
      public:
        virtual ~ServiceScopeFactory() = default;

        virtual std::shared_ptr<ServiceScope> create_scope() = 0;

        virtual std::shared_ptr<ServiceScope> create_scope(Name name) = 0;

        virtual std::shared_ptr<ServiceScope> create_scope(Delegate<void(ServiceCollection &)> configure) = 0;

        virtual std::shared_ptr<ServiceScope> create_scope(Name name,
                                                           Delegate<void(ServiceCollection &)> configure) = 0;
    };
} // namespace retro
