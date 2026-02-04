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
    private readonly List<SceneObject> _sceneObjects = [];

    public void Start()
    {
        Logger.Info("Starting game runner.");

        var texture = Asset.Load<Texture>(new AssetPath("graphics", "eevee.png"));

        _sceneObjects.Add(
            new Sprite
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
        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Dispose();
        }
    }
}
