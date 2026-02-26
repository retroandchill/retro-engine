// // @file Program.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using RetroEngine;
using RetroEngine.Game.Sample;
using RetroEngine.Host;

Console.WriteLine(Environment.GetEnvironmentVariable("PATH"));

var builder = new EngineHostBuilder();
builder.Services.AddSingleton<IGameSession, GameRunner>();

await using var host = builder.Build();
await host.RunAsync();
