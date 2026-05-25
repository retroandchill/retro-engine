// @file RootWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Events;
using RetroEngine.World;

namespace RetroEngine.UI;

public sealed class UiRoot : IDisposable
{
    private bool _disposed;

    private readonly EventManager _eventManager;
    private readonly Scene _scene;
    private readonly Viewport _viewport;

    public int ZOrder
    {
        get => _viewport.ZOrder;
        set => _viewport.ZOrder = value;
    }

    public UiRoot(EventManager eventManager, SceneManager sceneManager, ViewportManager viewportManager)
    {
        _eventManager = eventManager;
        _scene = new Scene(sceneManager);
        _viewport = new Viewport(viewportManager) { Scene = _scene };
        _eventManager.WindowResized += OnWindowResized;
    }

    private void OnWindowResized(ulong windowId, int width, int height) { }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _eventManager.WindowResized -= OnWindowResized;
        _scene.Dispose();
        _viewport.Dispose();
    }
}
