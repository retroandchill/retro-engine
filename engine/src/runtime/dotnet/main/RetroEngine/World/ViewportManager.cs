// // @file ViewportManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.World;

[NativeMarshalling(typeof(ViewportManagerMarshaller))]
public sealed partial class ViewportManager : IDisposable
{
    internal IntPtr NativeHandle { get; private set; } = NativeCreate();
    private readonly List<Viewport> _viewports = [];

    public bool Disposed => NativeHandle == IntPtr.Zero;

    ~ViewportManager()
    {
        Dispose();
    }

    internal void AddViewport(Viewport viewport)
    {
        _viewports.Add(viewport);
    }

    internal void RemoveViewport(Viewport viewport)
    {
        _viewports.Remove(viewport);
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
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_manager_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_manager_destroy")]
    private static partial void NativeDestroy(ViewportManager ptr);
}

[CustomMarshaller(typeof(ViewportManager), MarshalMode.ManagedToUnmanagedIn, typeof(ViewportManagerMarshaller))]
public static class ViewportManagerMarshaller
{
    public static IntPtr ConvertToUnmanaged(ViewportManager? manager) => manager?.NativeHandle ?? IntPtr.Zero;
}
