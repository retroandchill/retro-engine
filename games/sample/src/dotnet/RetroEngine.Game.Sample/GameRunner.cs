// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using RetroEngine.Assets;
using RetroEngine.Assets.Textures;
using RetroEngine.Async;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Platform;
using RetroEngine.Tickables;
using RetroEngine.World;
using Serilog;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner(
    AssetManager assetManager,
    IHostApplicationLifetime lifetime,
    TickManager tickManager,
    SceneManager sceneManager,
    ViewportManager viewportManager
) : AsyncGameSession(lifetime)
{
    protected override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting game runner.");

        await Engine.Instance.CreateMainWindowAsync(
            "Retro Engine",
            1280,
            720,
            WindowFlags.Resizable | WindowFlags.Vulkan,
            cancellationToken
        );

        using var scene1 = new Scene(sceneManager);
        using var scene2 = new Scene(sceneManager);

        using var viewport1 = new Viewport(viewportManager);
        viewport1.Scene = scene1;
        viewport1.CameraPivot = new Vector2F(0.5f, 0.5f);

        using var viewport2 = new Viewport(viewportManager);
        viewport2.Scene = scene2;
        viewport2.CameraPivot = new Vector2F(0.5f, 0.5f);
        viewport2.ZOrder = -1;

        var eeveeTexture = await assetManager.LoadAssetAsync<Texture>(
            new AssetPath("graphics", "133.png"),
            cancellationToken
        );
        if (eeveeTexture is null)
        {
            return 1;
        }
        var backgroundTexture = await assetManager.LoadAssetAsync<Texture>(
            new AssetPath("graphics", "background.png"),
            cancellationToken
        );

        using var flipbook = new SimpleFlipbook(scene1, eeveeTexture, tickManager, 10.0f);
        flipbook.Scale = new Vector2F(3, 3);
        flipbook.Tint = new Color(1, 1, 1);

        using var sprite = new Sprite(scene2);
        sprite.Texture = backgroundTexture;
        sprite.Pivot = new Vector2F(0.5f, 0.5f);
        sprite.ZOrder = -100000;

        await Task.Delay(Timeout.Infinite, cancellationToken);
        return 0;
    }
}
