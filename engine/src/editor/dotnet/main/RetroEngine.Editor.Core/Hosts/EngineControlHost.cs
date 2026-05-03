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

public sealed class EngineControlHost(RenderManager renderManager) : NativeControlHost, IDisposable
{
    public ulong WindowId
    {
        get;
        private set
        {
            if (field == value)
                return;

            field = value;
            _boundViewports.RemoveWhere(x => x.Disposed);
            foreach (var viewport in _boundViewports)
            {
                renderManager.BindViewportToWindow(viewport, field);
            }
        }
    }
    private PlatformWindowHandle _window;

    private readonly HashSet<Viewport> _boundViewports = [];

    public void BindViewport(Viewport viewport)
    {
        if (_boundViewports.Add(viewport))
        {
            renderManager.BindViewportToWindow(viewport, WindowId);
        }
    }

    public void UnbindViewport(Viewport viewport)
    {
        if (_boundViewports.Remove(viewport))
        {
            renderManager.BindViewportToWindow(viewport, 0);
        }
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

    public void Dispose()
    {
        if (WindowId == 0)
            return;

        renderManager.RemoveWindow(WindowId);
        _window = default;
        WindowId = 0;
    }
}
