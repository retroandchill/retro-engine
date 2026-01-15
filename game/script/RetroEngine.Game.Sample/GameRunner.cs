// // @file $FILE
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Logging;
using RetroEngine.SceneView;

namespace RetroEngine.Game.Sample;

public class GameRunner
{
    public static async Task<int> Main(CancellationToken cancellationToken)
    {
        Logger.Info("Starting game runner.");
        using var mainViewport = new Viewport();

        await Task.Delay(Timeout.Infinite, cancellationToken);
        return 0;
    }
}
