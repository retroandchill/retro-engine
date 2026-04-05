// // @file IThreadSync.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities.Async;

public interface IThreadSync
{
    int SyncThreadId { get; }

    public void RunOnPrimaryThread(Action action);

    public void RunOnPrimaryThread(Func<Task> action);

    public Task RunOnPrimaryThreadAsync(Action action);

    public Task RunOnPrimaryThreadAsync(Func<Task> action);
}
