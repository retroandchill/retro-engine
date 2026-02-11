// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Logging;
using RetroEngine.World;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner : IGameSession
{
    private Scene _scene = null!;
    private Viewport _viewport = null!;
    private readonly List<SimpleFlipbook> _sceneObjects = [];

    public void Start()
    {
        Logger.Info("Starting game runner.");

        _scene = new Scene();
        _viewport = new Viewport { Scene = _scene, CameraPivot = new Vector2F(0.5f, 0.5f) };

        var texture = Asset.Load<Texture>(new AssetPath("graphics", "133.png"));
        if (texture is null)
        {
            return;
        }

        _sceneObjects.Add(new SimpleFlipbook(_scene, texture, 10.0f) { Scale = new Vector2F(3, 3) });
    }

    public void Stop()
    {
        _viewport.Dispose();
        _scene.Dispose();
        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Dispose();
        }
    }
}
