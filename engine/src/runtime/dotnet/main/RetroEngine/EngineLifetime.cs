// // @file EngineLifetime.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;

namespace RetroEngine;

public sealed class EngineLifetime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _applicationStartedSource = new();
    public CancellationToken ApplicationStarted => _applicationStartedSource.Token;

    private readonly CancellationTokenSource _applicationStoppingSource = new();
    public CancellationToken ApplicationStopping => _applicationStoppingSource.Token;

    private readonly CancellationTokenSource _applicationStoppedSource = new();
    public CancellationToken ApplicationStopped => _applicationStoppedSource.Token;

    internal void NotifyStarted()
    {
        _applicationStartedSource.Cancel();
    }

    public void StopApplication()
    {
        _applicationStoppingSource.Cancel();
    }

    internal void NotifyStopped()
    {
        _applicationStoppedSource.Cancel();
    }
}
