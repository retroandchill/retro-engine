// @file EventManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Events;

public delegate void WindowResizedEventHandler(ulong windowId, int width, int height);

[NativeMarshalling(typeof(EventManagerMarshaller))]
public sealed partial class EventManager : IDisposable
{
    internal IntPtr NativeHandle { get; private set; } = NativeCreate();

    public event WindowResizedEventHandler? WindowResized
    {
        add
        {
            if (value is null)
                return;

            var userData = GCHandle.ToIntPtr(GCHandle.Alloc(value));
            unsafe
            {
                NativeWindowResizedAdd(
                    NativeHandle,
                    userData,
                    &InvokeWindowResized,
                    &DisposeDelegate,
                    &EqualsDelegates
                );
            }
        }
        remove
        {
            if (value is null)
                return;

            var userData = GCHandle.ToIntPtr(GCHandle.Alloc(value));
            unsafe
            {
                NativeWindowResizedRemove(
                    NativeHandle,
                    userData,
                    &InvokeWindowResized,
                    &DisposeDelegate,
                    &EqualsDelegates
                );
            }
        }
    }

    ~EventManager()
    {
        Dispose();
    }

    internal void PollEvents()
    {
        NativePollEvents(NativeHandle);
    }

    [UnmanagedCallersOnly]
    private static void InvokeWindowResized(IntPtr userData, ulong windowId, int width, int height)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var action = (WindowResizedEventHandler?)handle.Target;
        action?.Invoke(windowId, width, height);
    }

    [UnmanagedCallersOnly]
    private static void DisposeDelegate(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        handle.Free();
    }

    [UnmanagedCallersOnly]
    private static byte EqualsDelegates(IntPtr lhs, IntPtr rhs)
    {
        var lhsHandle = GCHandle.FromIntPtr(lhs);
        var rhsHandle = GCHandle.FromIntPtr(rhs);
        return Equals(lhsHandle.Target, rhsHandle.Target) ? (byte)1 : (byte)0;
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDispose(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_event_manager_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_event_manager_destroy")]
    private static partial void NativeDispose(IntPtr handle);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_event_manager_poll_events")]
    private static partial void NativePollEvents(IntPtr handle);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_event_manager_window_resized_add")]
    private static unsafe partial void NativeWindowResizedAdd(
        IntPtr handle,
        IntPtr userData,
        delegate* unmanaged<IntPtr, ulong, int, int, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_event_manager_window_resized_remove")]
    private static unsafe partial void NativeWindowResizedRemove(
        IntPtr handle,
        IntPtr userData,
        delegate* unmanaged<IntPtr, ulong, int, int, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );
}

[CustomMarshaller(typeof(EventManager), MarshalMode.ManagedToUnmanagedIn, typeof(EventManagerMarshaller))]
public static class EventManagerMarshaller
{
    public static IntPtr ConvertToUnmanaged(EventManager backend) => backend.NativeHandle;
}
