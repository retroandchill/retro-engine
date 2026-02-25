// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Hosting;
using RetroEngine;
using RetroEngine.Platform;

using var platformBackend = new PlatformBackend(flags: PlatformInitFlags.Video);
Console.WriteLine("Hello, World!");

var host = new EngineHost();
await host.RunAsync();
