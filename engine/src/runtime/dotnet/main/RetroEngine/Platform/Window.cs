// // @file Window.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;
using RetroEngine.Utilities;

namespace RetroEngine.Platform;

[NativeMarshalling(typeof(WindowMarshaller))]
public sealed partial class Window : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    internal Window(IntPtr nativeHandle, bool increaseRefCount)
    {
        NativeHandle = nativeHandle;
        if (increaseRefCount)
            NativeAddRef(this);
    }

    ~Window()
    {
        if (NativeHandle != IntPtr.Zero)
            throw new InvalidStateException(
                "Window was not disposed before it was finalized, "
                    + "this will lead to the window object being unable to be closed"
            );
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeSubRef(this);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_window_add_ref")]
    private static partial void NativeAddRef(Window ptr);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_window_sub_ref")]
    private static partial void NativeSubRef(Window ptr);
}

[CustomMarshaller(typeof(Window), MarshalMode.ManagedToUnmanagedIn, typeof(WindowMarshaller))]
public static class WindowMarshaller
{
    public static IntPtr ConvertToUnmanaged(Window? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
