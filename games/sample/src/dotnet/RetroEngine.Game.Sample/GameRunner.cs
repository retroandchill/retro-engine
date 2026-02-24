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
    private Scene _scene1 = null!;
    private Scene _scene2 = null!;
    private Viewport _viewport1 = null!;
    private Viewport _viewport2 = null!;
    private readonly List<IDisposable> _sceneObjects = [];

    public void Start()
    {
        Logger.Info("Starting game runner.");

        _scene1 = new Scene();
        _scene2 = new Scene();
        _viewport1 = new Viewport { Scene = _scene1, CameraPivot = new Vector2F(0.5f, 0.5f) };
        _viewport2 = new Viewport
        {
            Scene = _scene2,
            CameraPivot = new Vector2F(0.5f, 0.5f),
            ZOrder = -1,
        };

        var eeveeTexture = Asset.Load<Texture>(new AssetPath("graphics", "133.png"));
        if (eeveeTexture is null)
        {
            return;
        }
        var backgroundTexture = Asset.Load<Texture>(new AssetPath("graphics", "background.png"));

        _sceneObjects.Add(
            new SimpleFlipbook(_scene1, eeveeTexture, 10.0f) { Scale = new Vector2F(3, 3), Tint = new Color(1, 1, 1) }
        );
        _sceneObjects.Add(
            new Sprite(_scene2)
            {
                Texture = backgroundTexture,
                Pivot = new Vector2F(0.5f, 0.5f),
                ZOrder = -100000,
            }
        );
    }

    public void Stop()
    {
        _viewport1.Dispose();
        _viewport2.Dispose();
        _scene1.Dispose();
        _scene2.Dispose();
        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Dispose();
        }
    }
}
