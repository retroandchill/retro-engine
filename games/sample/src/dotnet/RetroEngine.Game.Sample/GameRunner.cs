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
using RetroEngine.Interaction;
using RetroEngine.Platform;
using RetroEngine.Rendering;
using RetroEngine.Rendering.Text;
using RetroEngine.Tickables;
using RetroEngine.UI;
using RetroEngine.UI.Containers;
using RetroEngine.UI.Display;
using RetroEngine.World;
using Serilog;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner(
    IAssetManager assetManager,
    IHostApplicationLifetime lifetime,
    RenderManager renderManager,
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

        using var scene1 = new Scene();
        using var scene2 = new Scene();

        using var viewport1 = new Viewport();
        viewport1.Scene = scene1;
        viewport1.CameraPivot = new Vector2F(0.5f, 0.5f);

        using var viewport2 = new Viewport();
        viewport2.Scene = scene2;
        viewport2.CameraPivot = new Vector2F(0.5f, 0.5f);
        viewport2.ZOrder = -1;

        var eeveeTexture = await Asset.LoadAsync<Texture>(new AssetPath("game", "graphics/133.png"), cancellationToken);
        var backgroundTexture = await Asset.LoadAsync<Texture>(
            new AssetPath("game", "graphics/background.png"),
            cancellationToken
        );
        var choiceTexture = await Asset.LoadAsync<Texture>(
            new AssetPath("game", "graphics/windows/choice_1.png"),
            cancellationToken
        );

        var textFont = await Asset.LoadAsync<Font>(
            new AssetPath("game", "fonts/roboto_regular.ttf"),
            cancellationToken
        );

        _ = new SimpleFlipbook(scene1, eeveeTexture, 10.0f) { Scale = new Vector2F(3, 3), Tint = new Color(1, 1, 1) };

        _ = new Sprite(scene2)
        {
            Texture = backgroundTexture,
            Pivot = new Vector2F(0.5f, 0.5f),
            ZOrder = -100000,
        };

        using var uiRoot = new UiRoot();
        uiRoot.ZOrder = 100000;

        var canvasPanel = new CanvasWidget();
        uiRoot.Content = canvasPanel;

        var window = new SpriteWidget()
        {
            Texture = choiceTexture,
            PreferredSize = new Vector2F(300, 124),
            DrawMode = SpriteDrawMode.Box,
            Margin = new Margin(14),
        };
        var windowSlot = canvasPanel.AddChild(window);
        windowSlot.AutoSize = true;
        windowSlot.LayoutData = windowSlot.LayoutData with
        {
            Anchors = new Anchors { Minimum = new Vector2F(0, 0.75f), Maximum = new Vector2F(1, 1) },
        };

        var textBlock = new TextBlockWidget();
        textBlock.Text = "Pokémon";
        textBlock.Font = textFont;
        textBlock.FontSize = 32;
        textBlock.Color = new Color(0, 0, 0);
        var textBlockSlot = canvasPanel.AddChild(textBlock);
        textBlockSlot.ZOrder = 1;
        textBlockSlot.LayoutData = windowSlot.LayoutData with
        {
            Anchors = new Anchors { Minimum = new Vector2F(0, 0.75f), Maximum = new Vector2F(1, 1) },
            Offsets = new Margin
            {
                Top = 14,
                Bottom = 14,
                Left = 14,
                Right = 14,
            },
        };

        await Task.WaitUntil(() => Input.IsDown(PhysicalKey.Space), cancellationToken);
        Log.Information("Space pressed.");

        await Task.Delay(Timeout.Infinite, cancellationToken);
        return 0;
    }
}
