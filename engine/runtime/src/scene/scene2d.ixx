//
// Created by fcors on 12/29/2025.
//
module;

#include "retro/core/exports.h"

export module retro.runtime:scene.scene2d;

import std;
import retro.core;
import :scene.entity;
import :scene.rendering;

namespace retro
{
    struct EntitySlot
    {
        uint32 dense_index{};
        uint32 generation{};
        bool alive{};
    };

    export class RETRO_API Scene2D
    {
      public:
        Scene2D() = default;
        ~Scene2D() = default;

        Scene2D(const Scene2D &) = delete;
        Scene2D(Scene2D &&) = default;
        Scene2D &operator=(const Scene2D &) = delete;
        Scene2D &operator=(Scene2D &&) = default;

        RenderProxyManager &render_proxy_manager()
        {
            return render_proxy_manager_;
        }

        Entity &create_entity(const Transform &transform = {}) noexcept;

        void destroy_entity(EntityID id);

    private:
        std::vector<std::unique_ptr<Entity>> entities_;
        std::vector<EntitySlot> slots_;
        std::vector<uint32> free_list_;

        RenderProxyManager render_proxy_manager_;
    };
} // namespace retro