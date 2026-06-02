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

    public bool IsDown(DigitalInput input)
    {
        ThrowIfDisposed();
        return NativeIsDown(NativeHandle, input);
    }

    public bool IsAnyDown(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeIsAnyDown(NativeHandle, buttons, buttons.Length);
    }

    public bool AllAreDown(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeAreAllDown(NativeHandle, buttons, buttons.Length);
    }

    public bool AreNoneDown(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeAreNoneDown(NativeHandle, buttons, buttons.Length);
    }

    public bool WasPressed(DigitalInput input)
    {
        ThrowIfDisposed();
        return NativeWasPressed(NativeHandle, input);
    }

    public bool WasAnyPressed(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeWasAnyPressed(NativeHandle, buttons, buttons.Length);
    }

    public bool WereAllPressed(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeWereAllPressed(NativeHandle, buttons, buttons.Length);
    }

    public bool WereNonePressed(params ReadOnlySpan<DigitalInput> buttons)
    {
        ThrowIfDisposed();
        return NativeWereNonePressed(NativeHandle, buttons, buttons.Length);
    }

    public float GetAnalogueValue(AnalogueInput input)
    {
        ThrowIfDisposed();
        return NativeGetAnalogueValue(NativeHandle, input);
    }

    public float GetAnalogueValues(params ReadOnlySpan<AnalogueInput> inputs)
    {
        ThrowIfDisposed();
        return NativeGetAnalogueValues(NativeHandle, inputs, inputs.Length);
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
    private static partial bool NativeIsDown(IntPtr handle, DigitalInput key);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_is_any_down")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeIsAnyDown(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_are_all_down")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeAreAllDown(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_are_none_down")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeAreNoneDown(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_was_pressed")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeWasPressed(IntPtr handle, DigitalInput key);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_was_any_pressed")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeWasAnyPressed(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_were_all_pressed")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeWereAllPressed(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_were_none_pressed")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool NativeWereNonePressed(IntPtr handle, ReadOnlySpan<DigitalInput> keys, int length);

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

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_get_analogue_value")]
    private static partial float NativeGetAnalogueValue(IntPtr handle, AnalogueInput input);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_input_manager_get_analogue_values")]
    private static partial float NativeGetAnalogueValues(IntPtr handle, ReadOnlySpan<AnalogueInput> inputs, int length);
}

[CustomMarshaller(typeof(InputManager), MarshalMode.ManagedToUnmanagedIn, typeof(InputManagerMarshaller))]
public static class InputManagerMarshaller
{
    public static IntPtr ConvertToUnmanaged(InputManager? manager) => manager?.NativeHandle ?? IntPtr.Zero;
}
