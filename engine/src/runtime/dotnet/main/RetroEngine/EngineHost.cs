// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetroEngine.Tickables;

namespace RetroEngine;

public sealed class EngineHost : IHost, IAsyncDisposable
{
    private readonly EngineLifetime _lifetime;
    private readonly ImmutableArray<IHostedService> _hostedServices;
    private readonly IGameSession? _gameSession;

    public IServiceProvider Services { get; }

    public EngineHost(Engine engine, IServiceProvider serviceProvider, EngineLifetime lifetime)
    {
        _lifetime = lifetime;
        _hostedServices = [.. serviceProvider.GetServices<IHostedService>()];
        _gameSession = serviceProvider.GetService<IGameSession>();
        Services = serviceProvider;

        var tickManager = serviceProvider.GetRequiredService<TickManager>();

        _lifetime.ApplicationStopping.Register(() => tickManager.ThreadSync.RunOnPrimaryThread(() => StopAsync()));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _gameSession?.Start();
        await Task.WhenAll(_hostedServices.Select(service => service.StartAsync(cancellationToken)));
        _lifetime.NotifyStarted();
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _gameSession?.Stop();
        await Task.WhenAll(_hostedServices.Select(service => service.StopAsync(cancellationToken)));
        _lifetime.NotifyStopped();
    }

    public void Dispose()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        switch (Services)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
