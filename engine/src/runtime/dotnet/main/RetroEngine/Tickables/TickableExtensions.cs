// @file TickableExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Tickables;

public static class TickableExtensions
{
    extension(Task)
    {
        public static Task WaitFrame(ulong frameCount = 1, CancellationToken cancellationToken = default)
        {
            if (frameCount == 0)
            {
                return Task.CompletedTask;
            }

            var awaiter = new TickAwait(frameCount, cancellationToken);
            return awaiter.Task;
        }

        public static async Task Timeline(
            float duration,
            Action<float> onTick,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(0, duration);

            if (duration == 0)
            {
                return;
            }

            var timeline = new Timeline(duration, onTick, cancellationToken);
            using var tickHandle = new TickHandle(timeline);
            await timeline.Task;
        }
    }
}
