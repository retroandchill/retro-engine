// // @file LockingEnumerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;

namespace RetroEngine.Utilities.Collections;

internal sealed class LockingEnumerable<T>(IEnumerable<T> inner, Lock locker) : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => new LockingEnumerator<T>(inner.GetEnumerator(), locker);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class LockingEnumerator<T> : IEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    private readonly Lock _lock;

    object? IEnumerator.Current => Current;
    public T Current => _inner.Current;

    public LockingEnumerator(IEnumerator<T> inner, Lock locker)
    {
        _inner = inner;
        _lock = locker;
        _lock.Enter();
    }

    public bool MoveNext()
    {
        return _inner.MoveNext();
    }

    public void Reset()
    {
        _inner.Reset();
    }

    public void Dispose()
    {
        _inner.Dispose();
        _lock.Exit();
    }
}

public static class LockingEnumerable
{
    public static IEnumerable<T> Lock<T>(this IEnumerable<T> source, Lock locker)
    {
        return new LockingEnumerable<T>(source, locker);
    }
}
