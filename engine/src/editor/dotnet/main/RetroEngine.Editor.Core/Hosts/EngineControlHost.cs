// // @file EngineControlHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Platform;
using PropertyGenerator.Avalonia;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Hosts;

public sealed partial class EngineControlHost : NativeControlHost
{
    [GeneratedDirectProperty]
    public partial IEngineRendererProvider? Provider { get; set; }

    private IEngineRendererProvider? _activeProvider;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (Provider is null)
            return base.CreateNativeControlCore(parent);

        _activeProvider = Provider;
        return Provider.CreateNativeWindow(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        base.DestroyNativeControlCore(control);
        _activeProvider?.DestroyWindow();
        _activeProvider = null;
    }
}
