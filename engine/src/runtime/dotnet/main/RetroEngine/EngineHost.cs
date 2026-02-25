// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;

namespace RetroEngine;

public sealed class EngineHostLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted { get; }
    public CancellationToken ApplicationStopping { get; }
    public CancellationToken ApplicationStopped { get; }

    public void StopApplication()
    {
        throw new NotImplementedException();
    }
}

public sealed class EngineHost : IHost, IAsyncDisposable
{
    public IServiceProvider Services { get; }
    private IntPtr _platformBackend;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}
