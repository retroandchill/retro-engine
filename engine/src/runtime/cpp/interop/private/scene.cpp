/**
 * @file scene.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include "retro/core/exports.h"

import retro.core.util.color;
import retro.core.math.vector;
import retro.core.math.matrix;
import retro.core.math.transform;
import retro.core.memory.ref_counted_ptr;
import retro.runtime.world.scene;
import retro.runtime.world.viewport;
import retro.runtime.world.scene_node;
import retro.runtime.rendering.objects.geometry;
import retro.runtime.rendering.objects.sprite;
import retro.runtime.engine;
import std;
import retro.interop.interop_error;

namespace retro
{
    struct NativeTransform2f
    {
        Vector2f position{};
        float rotation = 0;
        Vector2f scale{};
    };

    struct NativeCameraLayout
    {
        Vector2f position{};
        Vector2f pivot{};
        float rotation = 0;
        float zoom = 0;
    };

    namespace
    {
        Transform2f from_c(const NativeTransform2f &transform)
        {
            const Scale2f scale{transform.scale};
            const Quaternion2f rotation{transform.rotation};

            const auto matrix = Matrix2x2f{rotation} * Matrix2x2f{scale};
            return Transform2f{matrix, transform.position};
        }

        CameraLayout from_c(const NativeCameraLayout &layout)
        {
            return CameraLayout{.position = layout.position,
                                .pivot = layout.pivot,
                                .rotation = Quaternion2f{layout.rotation},
                                .zoom = layout.zoom};
        }
    } // namespace
} // namespace retro

extern "C"
{
    RETRO_API retro::SceneManager *retro_scene_manager_create()
    {
        return new retro::SceneManager{};
    }

    RETRO_API void retro_scene_manager_destroy(const retro::SceneManager *manager)
    {
        delete manager;
    }

    RETRO_API retro::Scene *retro_scene_create(retro::SceneManager *manager, retro::InteropError *error)
    {
        return retro::try_execute([manager] { return std::addressof(manager->create_scene()); }, *error);
    }

    RETRO_API void retro_scene_destroy(retro::SceneManager *manager, retro::Scene *scene)
    {
        manager->destroy_scene(*scene);
    }

    RETRO_API retro::ViewportManager *retro_viewport_manager_create()
    {
        return new retro::ViewportManager{};
    }

    RETRO_API void retro_viewport_manager_destroy(const retro::ViewportManager *manager)
    {
        delete manager;
    }

    RETRO_API retro::Viewport *retro_viewport_create(retro::ViewportManager *viewport_manager,
                                                     retro::InteropError *error)
    {

        return retro::try_execute([viewport_manager] { return std::addressof(viewport_manager->create_viewport()); },
                                  *error);
    }

    RETRO_API void retro_viewport_destroy(retro::ViewportManager *viewport_manager, retro::Viewport *viewport)
    {
        viewport_manager->destroy_viewport(*viewport);
    }

    RETRO_API void retro_viewport_set_scene(retro::Viewport *viewport, retro::Scene *scene)
    {
        viewport->set_scene(scene);
    }

    RETRO_API void retro_viewport_set_screen_layout(retro::Viewport *viewport, const retro::ScreenLayout *layout)
    {
        viewport->set_screen_layout(*layout);
    }

    RETRO_API void retro_viewport_set_camera_layout(retro::Viewport *viewport, const retro::NativeCameraLayout *layout)
    {
        viewport->set_camera_layout(retro::from_c(*layout));
    }

    RETRO_API void retro_viewport_set_z_order(retro::Viewport *viewport, const std::int32_t z_order)
    {
        viewport->set_z_order(z_order);
    }

    RETRO_API void retro_node_dispose(retro::Scene *scene, retro::SceneNode *node)
    {
        scene->destroy_node(*node);
    }

    RETRO_API void retro_node_set_transform(retro::SceneNode *node, const retro::NativeTransform2f *transform)
    {
        node->set_transform(retro::from_c(*transform));
    }

    RETRO_API std::int32_t retro_node_set_z_order(retro::SceneNode *node, const std::int32_t z_order)
    {
        node->set_z_order(z_order);
        return node->z_order();
    }

    RETRO_API void retro_node_attach_to_parent(retro::SceneNode *node, retro::SceneNode *parent)
    {
        node->attach_to_parent(parent);
    }

    RETRO_API void retro_node_detach_from_parent(retro::SceneNode *node)
    {
        node->detach_from_parent();
    }

    RETRO_API retro::GeometryObject *retro_geometry_create(retro::Scene *scene)
    {
        return std::addressof(scene->create_node<retro::GeometryObject>());
    }

    RETRO_API void retro_geometry_set_type(retro::GeometryObject *node, const retro::GeometryType type)
    {
        node->set_geometry(type);
    }

    RETRO_API void retro_geometry_set_render_data(retro::GeometryObject *node,
                                                  const retro::Vertex *vertices,
                                                  const std::int32_t vertex_count,
                                                  std::uint32_t *indices,
                                                  const std::int32_t index_count)
    {
        node->set_geometry(std::make_shared<const retro::Geometry>(
            std::span{vertices, static_cast<std::size_t>(vertex_count)} | std::ranges::to<std::vector>(),
            std::span{indices, static_cast<std::size_t>(index_count)} | std::ranges::to<std::vector>()));
    }

    RETRO_API void retro_geometry_set_color(retro::GeometryObject *node, const retro::Color color)
    {
        node->set_color(color);
    }

    RETRO_API void retro_geometry_set_pivot(retro::GeometryObject *node, const retro::Vector2f pivot)
    {
        node->set_pivot(pivot);
    }

    RETRO_API void retro_geometry_set_size(retro::GeometryObject *node, const retro::Vector2f size)
    {
        node->set_size(size);
    }

    RETRO_API retro::Sprite *retro_sprite_create(retro::Scene *scene)
    {
        return std::addressof(scene->create_node<retro::Sprite>());
    }

    RETRO_API void retro_sprite_set_texture(retro::Sprite *node, retro::Texture *texture)
    {
        node->set_texture(retro::RefCountPtr<retro::Texture>::ref(texture));
    }

    RETRO_API void retro_sprite_set_tint(retro::Sprite *node, const retro::Color tint)
    {
        node->set_tint(tint);
    }

    RETRO_API void retro_sprite_set_pivot(retro::Sprite *node, const retro::Vector2f pivot)
    {
        node->set_pivot(pivot);
    }

    RETRO_API void retro_sprite_set_size(retro::Sprite *node, const retro::Vector2f size)
    {
        node->set_size(size);
    }

    RETRO_API void retro_sprite_set_uv_rect(retro::Sprite *node, const retro::UVs uv_rect)
    {
        node->set_uvs(uv_rect);
    }
}
