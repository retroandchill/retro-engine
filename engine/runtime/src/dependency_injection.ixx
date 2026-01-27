/**
 * @file dependency_injection.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <boost/di.hpp>

export module retro.runtime:dependency_injection;

export namespace boost
{
    namespace ext::di
    {
        using di::bind;
        using di::create;
        using di::injector;
        using di::make_injector;
        using di::named;
        using di::override;

        using di::deduce;
        using di::singleton;
        using di::unique;
    } // namespace ext::di

    // We need this so IntelliSense properly picks up the alias
    namespace di = di;
} // namespace boost
