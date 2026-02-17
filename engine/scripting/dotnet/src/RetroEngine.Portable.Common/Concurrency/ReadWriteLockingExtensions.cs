// // @file ReadWriteLockingExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Concurrency;

public readonly ref struct ReadLockScope(ReaderWriterLockSlim lockSlim) : IDisposable
{
    public void Dispose()
    {
        lockSlim.ExitReadLock();
    }
}

public readonly ref struct WriteLockScope(ReaderWriterLockSlim lockSlim) : IDisposable
{
    public void Dispose()
    {
        lockSlim.ExitWriteLock();
    }
}

public static class ReadWriteLockingExtensions
{
    extension(ReaderWriterLockSlim lockSlim)
    {
        public ReadLockScope EnterReadScope()
        {
            lockSlim.EnterReadLock();
            return new ReadLockScope(lockSlim);
        }

        public WriteLockScope EnterWriteScope()
        {
            lockSlim.EnterWriteLock();
            return new WriteLockScope(lockSlim);
        }
    }
}
