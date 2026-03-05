// // @file Program.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine;
using RetroEngine.Assets;
using RetroEngine.Game.Sample;
using RetroEngine.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration().WithEngineLog().CreateLogger();

var builder = new EngineBuilder();
builder
    .Services.AddSingleton<IAssetPackage>(provider =>
    {
        // TODO: Replace this with the real asset package system eventually
        var filesystem = provider.GetRequiredService<IFileSystem>();
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var graphicsFolder = Path.Join(Path.GetDirectoryName(assemblyPath), "graphics");
        return new FilesystemAssetPackage(filesystem, graphicsFolder, "graphics");
    })
    .AddSingleton<IGameSession, GameRunner>();

await using var host = builder.Build();
host.Run();
