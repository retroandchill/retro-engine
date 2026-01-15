// // @file GameContext.cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Host;

internal sealed class GameContext : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task<int> _mainTask;

    public GameContext(Func<CancellationToken, Task<int>> taskDelegate)
    {
        _mainTask = taskDelegate(_cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
