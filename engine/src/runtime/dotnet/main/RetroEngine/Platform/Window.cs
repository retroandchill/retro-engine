// // @file Window.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Platform;

[NativeMarshalling(typeof(WindowMarshaller))]
public sealed partial class Window : IDisposable
{
    internal IntPtr Handle { get; private set; }

    internal Window(IntPtr handle)
    {
        Handle = handle;
    }

    ~Window()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (Handle == IntPtr.Zero)
            return;

        NativeDestroy(Handle);
        Handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_window_destroy")]
    private static partial void NativeDestroy(IntPtr handle);
}

[CustomMarshaller(typeof(Window), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToNative))]
public static class WindowMarshaller
{
    public static class ManagedToNative
    {
        public static IntPtr ConvertToUnmanaged(Window window)
        {
            return window.Handle;
        }
    }
}
