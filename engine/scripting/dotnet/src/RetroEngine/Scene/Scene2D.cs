using RetroEngine.Core.Math;
using RetroEngine.Interop;

namespace RetroEngine.Scene;

public class Scene2D
{
    public static Scene2D Current { get; private set; } = new();

    internal int Generation { get; }

    private readonly Dictionary<ulong, Entity> _instantiatedEntities = [];

    public Entity CreateNewEntity(in Transform transform = default)
    {
        var nativeEntity = EntityExporter.CreateNewEntity(in transform, out var id);
        var entity = new Entity(this, id, nativeEntity);
        _instantiatedEntities.Add(id, entity);
        return entity;
    }

    internal void RemoveEntity(ulong id)
    {
        _instantiatedEntities.Remove(id);
    }
}
