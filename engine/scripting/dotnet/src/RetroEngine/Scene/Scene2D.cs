// @file $Scene2D.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using RetroEngine.Core.Math;
using RetroEngine.Interop;

namespace RetroEngine.Scene;

public class Scene2D
{
    public static Scene2D Current { get; private set; } = new();

    internal int Generation { get; }

    private readonly Dictionary<EntityId, Entity> _instantiatedEntities = [];

    public Entity CreateNewEntity(in Transform transform = default)
    {
        var entityId = EntityExporter.CreateNewEntity(in transform);
        var entity = new Entity(this, entityId);
        _instantiatedEntities.Add(entityId, entity);
        return entity;
    }

    internal void RemoveEntity(EntityId id)
    {
        _instantiatedEntities.Remove(id);
    }
}
