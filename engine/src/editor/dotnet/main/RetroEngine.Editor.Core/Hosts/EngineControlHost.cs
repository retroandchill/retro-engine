// // @file EngineControlHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Platform;
using RetroEngine.Platform;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.Editor.Core.Hosts;

public sealed class EngineControlHost(RenderManager renderManager) : NativeControlHost
{
    public ulong WindowId { get; private set; }
    private PlatformWindowHandle _window;

    public void BindViewport(Viewport viewport)
    {
        renderManager.BindViewportToWindow(viewport, WindowId);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        WindowId = renderManager.CreateWindowFromNative(FromPlatformHandle(parent));
        try
        {
            _window = renderManager.GetWindowById(WindowId);
            var platformName = _window.Backend switch
            {
                WindowBackend.Headless => "Headless",
                WindowBackend.SDL3 => "SDL3",
                _ => throw new InvalidOperationException($"Invalid backend: {_window.Backend}"),
            };
            return new PlatformHandle(_window.Handle, platformName);
        }
        catch
        {
            renderManager.RemoveWindow(WindowId);
            throw;
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        base.DestroyNativeControlCore(control);
        renderManager.RemoveWindow(WindowId);
        _window = default;
    }

    private static NativeWindowHandle FromPlatformHandle(IPlatformHandle handle)
    {
        var type = handle.HandleDescriptor switch
        {
            "HWND" => NativeWindowType.Win32Hwnd,
            "XID" => NativeWindowType.X11,
            "NSWindow" => NativeWindowType.CocoaWindow,
            "NSView" => NativeWindowType.CocoaView,
            _ => NativeWindowType.Unknown,
        };
        return new NativeWindowHandle(type, handle.Handle);
    }
}
