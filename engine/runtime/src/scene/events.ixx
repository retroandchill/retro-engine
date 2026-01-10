/**
 * @file events.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
export module retro.runtime:scene.events;

import std;

import :scene.actor_ptr;
import :scene.transform;

namespace retro
{
    export struct DestroyViewportEvent
    {
        ViewportID viewport_id;

        constexpr explicit DestroyViewportEvent(const ViewportID viewport_id) : viewport_id{viewport_id}
        {
        }
    };

    export struct DestroyRenderObject
    {
        RenderObjectID render_object_id;

        constexpr explicit DestroyRenderObject(const RenderObjectID render_object_id)
            : render_object_id{render_object_id}
        {
        }
    };

    export struct UpdateRenderObjectTransformEvent
    {
        RenderObjectID render_object_id;
        Transform transform;

        constexpr explicit UpdateRenderObjectTransformEvent(const RenderObjectID render_object_id,
                                                            const Transform &transform)
            : render_object_id{render_object_id}, transform{transform}
        {
        }
    };

    export using SceneEvent = std::variant<DestroyViewportEvent, DestroyRenderObject, UpdateRenderObjectTransformEvent>;

    template <typename, typename>
    struct IsVariantMember : std::false_type
    {
    };

    template <typename T, typename... Types>
    struct IsVariantMember<T, std::variant<Types...>> : std::disjunction<std::is_same<T, Types>...>
    {
    };

    export template <typename T>
    concept IsSceneEvent = IsVariantMember<T, SceneEvent>::value;
} // namespace retro
