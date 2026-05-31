// @file InputManager.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Core.Math;
using RetroEngine.Interop;

namespace RetroEngine.Interaction;

[NativeMarshalling(typeof(InputManagerMarshaller))]
public sealed partial class InputManager : IDisposable
{
    internal IntPtr NativeHandle { get; private set; } = NativeCreate();

    public Vector2F MousePosition { get; private set; }

    public Vector2F MouseDelta { get; private set; }

    public Vector2F MouseScrollDelta { get; private set; }

    internal void PollEvents(ulong frameCount)
    {
        ThrowIfDisposed();
        NativePollEvents(NativeHandle, frameCount);

        NativeGetMousePosition(
            NativeHandle,
            out var positionX,
            out var positionY,
            out var deltaX,
            out var deltaY,
            out var scrollDeltaX,
            out var scrollDeltaY
        );
        MousePosition = new Vector2F(positionX, positionY);
        MouseDelta = new Vector2F(deltaX, deltaY);
        MouseScrollDelta = new Vector2F(scrollDeltaX, scrollDeltaY);
    }

    public bool IsDown(LogicalKey key)
    {
        ThrowIfDisposed();
        return NativeIsDown(NativeHandle, key);
    }

    public bool IsDown(PhysicalKey key)
    {
        ThrowIfDisposed();
        return NativeIsDown(NativeHandle, key);
    }

    public bool IsDown(MouseButton button)
    {
        ThrowIfDisposed();
        return NativeIsDown(NativeHandle, button);
    }

    public bool WasPressed(LogicalKey key)
    {
        ThrowIfDisposed();
        return NativeWasPressed(NativeHandle, key);
    }

    public bool WasPressed(PhysicalKey key)
    {
        ThrowIfDisposed();
        return NativeWasPressed(NativeHandle, key);
    }

    public bool WasPressed(MouseButton button)
    {
        ThrowIfDisposed();
        return NativeWasPressed(NativeHandle, button);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(NativeHandle == IntPtr.Zero, this);
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_destroy")]
    private static partial void NativeDestroy(IntPtr handle);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_poll_events")]
    private static partial void NativePollEvents(IntPtr handle, ulong frameCount);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_is_down")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeIsDown(IntPtr handle, ButtonInput key);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_was_pressed")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeWasPressed(IntPtr handle, ButtonInput key);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_get_mouse_position")]
    private static partial void NativeGetMousePosition(
        IntPtr handle,
        out float positionX,
        out float positionY,
        out float deltaX,
        out float deltaY,
        out float scrollDeltaX,
        out float scrollDeltaY
    );
}

[CustomMarshaller(typeof(InputManager), MarshalMode.ManagedToUnmanagedIn, typeof(InputManagerMarshaller))]
public static class InputManagerMarshaller
{
    public static IntPtr ConvertToUnmanaged(InputManager? manager) => manager?.NativeHandle ?? IntPtr.Zero;
}
