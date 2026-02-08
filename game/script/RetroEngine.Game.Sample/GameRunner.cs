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
    private readonly List<SceneObject> _sceneObjects = [];

    public void Start()
    {
        Logger.Info("Starting game runner.");

        _scene = new Scene();
        _viewport = new Viewport() { Scene = _scene };

        var texture = Asset.Load<Texture>(new AssetPath("graphics", "eevee.png"));

        _sceneObjects.Add(
            new Sprite(_scene)
            {
                Texture = texture,
                Position = new Vector2F(640f, 360f),
                Pivot = new Vector2F(0.5f, 0.5f),
                Tint = new Color(1, 1, 1),
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
