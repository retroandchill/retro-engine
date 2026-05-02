// // @file SemaphoreExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities.Concurrency;

public readonly struct SemaphoreScope(SemaphoreSlim semaphore) : IDisposable
{
    public void Dispose()
    {
        semaphore.Release();
    }
}

public static class SemaphoreExtensions
{
    extension(SemaphoreSlim semaphore)
    {
        public SemaphoreScope EnterScope()
        {
            semaphore.Wait();
            return new SemaphoreScope(semaphore);
        }

        public async ValueTask<SemaphoreScope> EnterScopeAsync(CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new SemaphoreScope(semaphore);
        }
    }
}
