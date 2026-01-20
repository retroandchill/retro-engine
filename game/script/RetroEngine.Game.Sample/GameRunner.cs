// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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

        const int width = 1280 / 100 + 1;
        const int height = 720 / 100 + 1;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var index = i + j * width;
                var r = (index & 1) != 0 ? 1.0f : 0.0f;
                var g = (index & 2) != 0 ? 1.0f : 0.0f;
                var b = (index & 4) != 0 ? 1.0f : 0.0f;

                _ = new Quad(_viewport)
                {
                    Position = new Vector2F(i * 100.0f, j * 100.0f),
                    Size = new Vector2F(100.0f, 100.0f),
                    Color = new Color(r, g, b),
                };
            }
        }
    }

    public void Stop()
    {
        _viewport?.Dispose();
    }
}
