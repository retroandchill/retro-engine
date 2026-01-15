// // @file $FILE
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Logging;
using RetroEngine.SceneView;

namespace RetroEngine.Game.Sample;

public sealed class GameRunner : IGameSession
{
    private Viewport? _viewport;

    public void Start()
    {
        if (_viewport is not null)
            throw new InvalidOperationException("Game session is already running.");

        Logger.Info("Starting game runner.");
        _viewport = new Viewport();
    }

    public void Stop()
    {
        _viewport?.Dispose();
    }
}
