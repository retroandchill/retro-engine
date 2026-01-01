using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Strings;

namespace RetroEngine.Scene;

internal readonly record struct ComponentKey(Type ComponentType, Name Identifier = default);

public sealed class Entity : IDisposable
{
    private ulong Id { get; }
    private IntPtr _entityPtr;
    internal int Generation { get; }
    private readonly Dictionary<ComponentKey, Component> _components = new();

    private static readonly int TransformOffset = EntityExporter.GetEntityTransformOffset();

    public Transform Transform
    {
        get
        {
            ThrowIfInvalid();
            unsafe
            {
                return *(Transform*)IntPtr.Add(_entityPtr, TransformOffset);
            }
        }
        set
        {
            ThrowIfInvalid();
            unsafe
            {
                *(Transform*)IntPtr.Add(_entityPtr, TransformOffset) = value;
            }
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

    internal Entity(Scene2D scene, ulong id, IntPtr entityPtr)
    {
        Id = id;
        _entityPtr = entityPtr;
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
        if (_entityPtr != IntPtr.Zero && Generation == Scene2D.Current.Generation)
            return;
        throw new ObjectDisposedException(nameof(Entity));
    }

    public void Dispose()
    {
        if (_entityPtr == IntPtr.Zero || Generation != Scene2D.Current.Generation)
            return;

        EntityExporter.RemoveEntityFromScene(_entityPtr);
        Scene2D.Current.RemoveEntity(Id);
        _entityPtr = IntPtr.Zero;
    }
}
