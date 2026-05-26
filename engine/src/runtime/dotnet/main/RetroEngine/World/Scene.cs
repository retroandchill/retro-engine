// // @file Scene.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.World;

[NativeMarshalling(typeof(SceneMarshaller))]
public sealed partial class Scene : IDisposable
{
    internal static SceneManager? Manager { get; set; }
    internal IntPtr NativeHandle { get; private set; }
    private readonly List<SceneObject> _objects = [];

    public bool Disposed => NativeHandle == IntPtr.Zero || Manager is null;

    public IReadOnlyList<SceneObject> Objects => _objects;

    public Scene()
    {
        if (Manager is null)
            throw new InvalidOperationException("SceneManager is not initialized.");

        NativeHandle = NativeCreate(Manager, out var error);
        error.ThrowIfError();
        Manager.AddScene(this);
    }

    public IEnumerable<SceneObject> GetDirectChildren()
    {
        return _objects.Where(obj => obj.Parent is null);
    }

    internal void AddObject(SceneObject obj)
    {
        _objects.Add(obj);
    }

    internal void RemoveObject(SceneObject obj)
    {
        var index = _objects.IndexOf(obj);
        if (index == -1)
            return;

        _objects[index] = _objects[^1];
        _objects.RemoveAt(_objects.Count - 1);
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        DisposeManagedResources();
        if (Manager is not null)
        {
            NativeDestroy(Manager, this);
            Manager.RemoveScene(this);
        }

        NativeHandle = IntPtr.Zero;
    }

    internal void DisposeManagedResources()
    {
        foreach (var obj in _objects)
        {
            obj.DisposeManagedResources();
        }
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_scene_create")]
    private static partial IntPtr NativeCreate(SceneManager ptr, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_scene_destroy")]
    private static partial void NativeDestroy(SceneManager manager, Scene scene);
}

[CustomMarshaller(typeof(Scene), MarshalMode.ManagedToUnmanagedIn, typeof(SceneMarshaller))]
public static class SceneMarshaller
{
    public static IntPtr ConvertToUnmanaged(Scene? scene) => scene?.NativeHandle ?? IntPtr.Zero;
}
