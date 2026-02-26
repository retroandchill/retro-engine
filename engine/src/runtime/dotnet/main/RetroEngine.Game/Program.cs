// See https://aka.ms/new-console-template for more information

using RetroEngine.Host;

Console.WriteLine("Hello, World!");

var builder = new EngineHostBuilder();

await using var host = builder.Build();
await host.RunAsync();
