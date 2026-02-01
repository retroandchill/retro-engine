// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Logging;
using RetroEngine.SceneView;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner : IGameSession
{
    private Viewport? _viewport;

    public void Start()
    {
        if (_viewport is not null)
            throw new InvalidOperationException("Game session is already running.");

        Logger.Info("Starting game runner.");
        _viewport = new Viewport(new Vector2F(1280, 720));

        var texture = Asset.Load<Texture>(new AssetPath("graphics", "eevee.png"));

        _ = new Sprite(_viewport)
        {
            Texture = texture,
            Position = new Vector2F(640f, 360f),
            Size = new Vector2F(100.0f, 100.0f),
            Pivot = new Vector2F(0.5f, 0.5f),
            Tint = new Color(1, 0, 0),
        };
    }

    public void Stop()
    {
        _viewport?.Dispose();
    }
}
