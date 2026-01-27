/**
 * @file entt.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#define ENTT_STANDARD_CPP
#include <entt/entity/registry.hpp>
#include <entt/entt.hpp>

export module retro.runtime:entt;

export namespace entt
{
    using entt::entity;
    using entt::id_type;
    using entt::null;
    using entt::registry;
    using entt::sparse_set;

    using entt::operator!=;

    namespace internal
    {
        using internal::operator!=;
    }
} // namespace entt
