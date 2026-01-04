// @file $Entity.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Runtime.InteropServices;
using RetroEngine.Binds;
using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Strings;

namespace RetroEngine.Scene;

internal readonly record struct ComponentKey(Type ComponentType, Name Identifier = default);

[BlittableType("retro::EntityID", CppModule = "retro.runtime")]
[StructLayout(LayoutKind.Sequential)]
public readonly record struct EntityId(uint Index, uint Generation);

public sealed class Entity : IDisposable
{
    private EntityId Id { get; }
    internal int Generation { get; }
    private bool _disposed;
    private readonly Dictionary<ComponentKey, Component> _components = new();

    public Transform Transform
    {
        get
        {
            ThrowIfInvalid();
            return EntityExporter.GetEntityTransform(Id);
        }
        set
        {
            ThrowIfInvalid();
            EntityExporter.SetEntityTransform(Id, in value);
        }
    }

    public Vector2F Location
    {
        get => Transform.Position;
        set => Transform = Transform with { Position = value };
    }

    public float Rotation
    {
        get => Transform.Rotation;
        set => Transform = Transform with { Rotation = value };
    }

    public Vector2F Scale
    {
        get => Transform.Scale;
        set => Transform = Transform with { Scale = value };
    }

    internal Entity(Scene2D scene, EntityId id)
    {
        Id = id;
        Generation = scene.Generation;
    }

    public T GetComponent<T>(Name name = default)
        where T : Component
    {
        ThrowIfInvalid();
        if (_components.TryGetValue(new ComponentKey(typeof(T), name), out var component))
        {
            return (T)component;
        }

        throw new InvalidOperationException($"Entity {Id} does not have a component of type {typeof(T)}");
    }

    public T AddComponent<T>(Name name = default)
        where T : Component
    {
        throw new NotImplementedException();
    }

    private void ThrowIfInvalid()
    {
        if (_disposed && Generation == Scene2D.Current.Generation)
            return;
        throw new ObjectDisposedException(nameof(Entity));
    }

    public void Dispose()
    {
        if (_disposed || Generation != Scene2D.Current.Generation)
            return;

        EntityExporter.RemoveEntityFromScene(Id);
        Scene2D.Current.RemoveEntity(Id);
        _disposed = true;
    }
}
