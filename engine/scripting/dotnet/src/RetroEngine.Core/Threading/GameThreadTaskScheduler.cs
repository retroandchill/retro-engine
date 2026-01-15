// // @file GameThreadTaskScheduler.cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Core.Threading;

public class GameThreadTaskScheduler : TaskScheduler
{
    private readonly GameThreadSynchronizationContext _context;

    public GameThreadTaskScheduler(GameThreadSynchronizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    protected override void QueueTask(Task task)
    {
        _context.Post(
            static s =>
            {
                var (scheduler, t) = ((GameThreadTaskScheduler, Task))s!;
                scheduler.TryExecuteTask(t);
            },
            (this, task)
        );
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return _context.IsOnGameThread && TryExecuteTask(task);
    }

    protected override IEnumerable<Task>? GetScheduledTasks()
    {
        return null;
    }
}
