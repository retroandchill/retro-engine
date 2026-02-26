// // @file EngineHostLifetime.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;

namespace RetroEngine.Host;

public sealed class EngineHostLifetime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _applicationStartedSource = new();
    private readonly CancellationTokenSource _applicationStoppingSource = new();
    private readonly CancellationTokenSource _applicationStoppedSource = new();

    public CancellationToken ApplicationStarted => _applicationStartedSource.Token;
    public CancellationToken ApplicationStopping => _applicationStoppingSource.Token;
    public CancellationToken ApplicationStopped => _applicationStoppedSource.Token;

    internal void NotifyStarted() => _applicationStartedSource.Cancel();

    internal void NotifyStopped() => _applicationStoppingSource.Cancel();

    public void StopApplication()
    {
        _applicationStoppingSource.Cancel();
    }
}
