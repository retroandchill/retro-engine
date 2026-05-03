// // @file EngineRendererProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Platform;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Hosts;
using RetroEngine.Platform;
using RetroEngine.Rendering;

namespace RetroEngine.Editor.Core.ViewModels;

public interface IEngineRendererProvider
{
    ulong WindowId { get; }

    event Action<ulong> OnWindowCreated;

    event Action<ulong> OnWindowDestroyed;

    IPlatformHandle CreateNativeWindow(IPlatformHandle parent);

    void DestroyWindow();
}

[ViewModelFor<EngineControlHost>]
public sealed partial class EngineRendererProvider(RenderManager renderManager) : IEngineRendererProvider
{
    public ulong WindowId { get; private set; }
    public event Action<ulong>? OnWindowCreated;
    public event Action<ulong>? OnWindowDestroyed;

    public IPlatformHandle CreateNativeWindow(IPlatformHandle parent)
    {
        IPlatformHandle result;
        WindowId = renderManager.CreateWindowFromNative(FromPlatformHandle(parent));
        try
        {
            var window = renderManager.GetWindowById(WindowId);
            var platformType = window.Backend switch
            {
                WindowBackend.Headless => "Headless",
                WindowBackend.SDL3 => "SDL3",
                _ => throw new InvalidOperationException($"Invalid backend: {window.Backend}"),
            };
            result = new PlatformHandle(window.Handle, platformType);
        }
        catch
        {
            renderManager.RemoveWindow(WindowId);
            WindowId = 0;
            throw;
        }

        OnWindowCreated?.Invoke(WindowId);
        return result;
    }

    public void DestroyWindow()
    {
        OnWindowDestroyed?.Invoke(WindowId);
        renderManager.RemoveWindow(WindowId);
        WindowId = 0;
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
