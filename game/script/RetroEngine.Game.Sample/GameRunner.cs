// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Core.Math;
using RetroEngine.Logging;
using RetroEngine.World;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner : IGameSession
{
    private Scene _scene = null!;
    private Viewport _viewport = null!;
    private readonly List<IDisposable> _sceneObjects = [];

    public void Start()
    {
        Logger.Info("Starting game runner.");

        _scene = new Scene();
        _viewport = new Viewport { Scene = _scene, CameraPivot = new Vector2F(0.5f, 0.5f) };

        var eeveeTexture = Asset.Load<Texture>(new AssetPath("graphics", "133.png"));
        if (eeveeTexture is null)
        {
            return;
        }
        var backgroundTexture = Asset.Load<Texture>(new AssetPath("graphics", "background.png"));

        _sceneObjects.Add(new SimpleFlipbook(_scene, eeveeTexture, 10.0f) { Scale = new Vector2F(3, 3) });
        _sceneObjects.Add(
            new Sprite(_scene)
            {
                Texture = backgroundTexture,
                Pivot = new Vector2F(0.5f, 0.5f),
                ZOrder = -100000,
            }
        );
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
