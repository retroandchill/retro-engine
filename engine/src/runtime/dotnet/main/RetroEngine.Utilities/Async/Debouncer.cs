// // @file Debouncer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities.Async;

public sealed class Debouncer : IDisposable
{
    private CancellationTokenSource? _cts;

    public void Debounce(Action action, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        _ = Task.Delay(delay, linkedToken.Token)
            .ContinueWith(
                t =>
                {
                    if (t.IsCompletedSuccessfully)
                        action();
                },
                cancellationToken
            );
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
