// // @file SceneManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.World;

[RegisterSingleton]
[NativeMarshalling(typeof(SceneManagerMarshaller))]
public sealed partial class SceneManager : IDisposable
{
    internal IntPtr NativeHandle { get; private set; } = NativeCreate();
    private readonly List<Scene> _scenes = [];

    public bool Disposed => NativeHandle == IntPtr.Zero;

    public IReadOnlyList<Scene> Scenes
    {
        get
        {
            ThrowIfDisposed();
            return _scenes;
        }
    }

    internal void AddScene(Scene scene)
    {
        _scenes.Add(scene);
    }

    internal void RemoveScene(Scene scene)
    {
        _scenes.Remove(scene);
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDestroy(this);
        NativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_scene_manager_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_scene_manager_destroy")]
    private static partial void NativeDestroy(SceneManager ptr);
}

[CustomMarshaller(typeof(SceneManager), MarshalMode.ManagedToUnmanagedIn, typeof(SceneManagerMarshaller))]
public static class SceneManagerMarshaller
{
    public static IntPtr ConvertToUnmanaged(SceneManager scene) => scene.NativeHandle;
}
