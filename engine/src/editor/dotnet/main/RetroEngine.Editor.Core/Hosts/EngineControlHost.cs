// // @file EngineControlHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using RetroEngine.Platform;
using RetroEngine.Rendering;

namespace RetroEngine.Editor.Core.Hosts;

public sealed class EngineControlHost(RenderManager renderManager) : NativeControlHost
{
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var windowId = renderManager.CreateWindowFromNative(FromPlatformHandle(parent));
        try
        {
            return new WindowControlHandle(renderManager, windowId, renderManager.GetWindowById(windowId));
        }
        catch
        {
            renderManager.RemoveWindow(windowId);
            throw;
        }
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

    private sealed class WindowControlHandle(RenderManager renderManager, ulong windowId, PlatformWindowHandle handle)
        : INativeControlHostDestroyableControlHandle
    {
        public IntPtr Handle => handle.Handle;

        public string HandleDescriptor
        {
            get
            {
                return handle.Backend switch
                {
                    WindowBackend.Headless => "Headless",
                    WindowBackend.SDL3 => "SDL3",
                    _ => throw new InvalidOperationException($"Invalid backend: {handle.Backend}"),
                };
            }
        }

        public void Destroy()
        {
            renderManager.RemoveWindow(windowId);
        }
    }
}
