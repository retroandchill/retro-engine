/**
 * @file component.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module retro.runtime;

namespace retro
{
    Entity &Component::entity() const noexcept
    {
        return Engine::instance().scene().get_entity(entity_id_).value();
    }
} // namespace retro
