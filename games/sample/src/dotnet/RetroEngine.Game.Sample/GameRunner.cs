// @file GameRunner.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Microsoft.Extensions.Hosting;
using RetroEngine.Assets;
using RetroEngine.Async;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Platform;
using RetroEngine.Rendering;
using RetroEngine.Tickables;
using RetroEngine.World;
using Serilog;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner(
    IAssetManager assetManager,
    IHostApplicationLifetime lifetime,
    TickManager tickManager,
    RenderManager renderManager,
    SceneManager sceneManager,
    ViewportManager viewportManager,
    IFileSystem fileSystem
) : AsyncGameSession(lifetime)
{
    protected override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting game runner.");

        await assetManager.LoadPackageAsync(
            "game",
            fileSystem.Path.Join(fileSystem.Directory.GetCurrentDirectory(), "content"),
            cancellationToken
        );

        await renderManager.CreateMainWindowAsync(
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
            new AssetPath("game", "graphics/133.png"),
            cancellationToken
        );
        var backgroundTexture = await assetManager.LoadAssetAsync<Texture>(
            new AssetPath("game", "graphics/background.png"),
            cancellationToken
        );
        var choiceTexture = await assetManager.LoadAssetAsync<Texture>(
            new AssetPath("game", "graphics/windows/choice_1.png"),
            cancellationToken
        );

        using var flipbook = new SimpleFlipbook(scene1, eeveeTexture, tickManager, 10.0f);
        flipbook.Scale = new Vector2F(3, 3);
        flipbook.Tint = new Color(1, 1, 1);

        using var sprite = new Sprite(scene2);
        sprite.Texture = backgroundTexture;
        sprite.Pivot = new Vector2F(0.5f, 0.5f);
        sprite.ZOrder = -100000;

        using var window = new Sprite(scene1);
        window.Texture = choiceTexture;
        window.Scale = new Vector2F(2, 2);
        window.Size = new Vector2F(300, 124);
        window.Pivot = new Vector2F(0.5f, 1f);
        window.Rotation = 0.5f;
        window.ZOrder = 100000;
        window.Position = new Vector2F(0, 360);
        window.DrawMode = SpriteDrawMode.Box;
        window.Margin = new Margin(14);

        await Task.Delay(Timeout.Infinite, cancellationToken);
        return 0;
    }
}
