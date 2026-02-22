// // @file StrippedDictionary.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Concurrency;

namespace RetroEngine.Portable.Collections;

public sealed class StripedDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly record struct Bucket(Dictionary<TKey, TValue> Dictionary, ReaderWriterLockSlim Lock);

    private readonly Bucket[] _buckets;
    private readonly IEqualityComparer<TKey> _comparer;

    public StripedDictionary(int stripes, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stripes);
        _buckets = new Bucket[stripes];
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        for (var i = 0; i < stripes; i++)
        {
            _buckets[i] = new Bucket(new Dictionary<TKey, TValue>(comparer), new ReaderWriterLockSlim());
        }
    }

    public int Count =>
        _buckets.Sum(b =>
        {
            using var scope = b.Lock.EnterReadScope();
            return b.Dictionary.Count;
        });

    private int GetStripeIndex(TKey key)
    {
        if (_buckets.Length == 1)
            return 0;

        var hash = _comparer.GetHashCode(key);
        return (hash & 0x7fffffff) % _buckets.Length;
    }

    private TResult Read<TResult>(TKey key, Func<TKey, Dictionary<TKey, TValue>, TResult> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterReadScope();
        return func(key, bucket.Dictionary);
    }

    private TResult Read<TResult, TContext>(
        TContext context,
        TKey key,
        Func<TContext, TKey, Dictionary<TKey, TValue>, TResult> func
    )
        where TContext : allows ref struct
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterReadScope();
        return func(context, key, bucket.Dictionary);
    }

    private void Write(TKey key, Action<TKey, Dictionary<TKey, TValue>> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterWriteScope();
        func(key, bucket.Dictionary);
    }

    private TResult Write<TResult>(TKey key, Func<TKey, Dictionary<TKey, TValue>, TResult> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterWriteScope();
        return func(key, bucket.Dictionary);
    }

    private void ApplyUnlocked(TKey key, Action<TKey, Bucket> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        func(key, bucket);
    }

    private TResult ApplyUnlocked<TResult>(TKey key, Func<TKey, Bucket, TResult> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        return func(key, bucket);
    }

    public TValue this[TKey key]
    {
        get { return Read(key, (k, d) => d[k]); }
    }

    public TValue? GetValueOrDefault(TKey key)
    {
        return Read(key, (k, d) => d.GetValueOrDefault(k));
    }

    public bool GetAndApply(TKey key, Action<TValue> action)
    {
        return Read(
            key,
            (k, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                action(value);
                return true;
            }
        );
    }

    public bool GetAndApply<TContext>(TContext context, TKey key, Action<TContext, TValue> action)
        where TContext : allows ref struct
    {
        return Read(
            context,
            key,
            (ctx, k, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                action(ctx, value);
                return true;
            }
        );
    }

    public TValue GetOrAdd(TKey key, Func<TValue> producer)
    {
        return ApplyUnlocked(
            key,
            (k, b) =>
            {
                using (b.Lock.EnterReadScope())
                {
                    if (b.Dictionary.TryGetValue(k, out var value))
                        return value;
                }

                using var scope = b.Lock.EnterWriteScope();
                if (b.Dictionary.TryGetValue(key, out var v))
                {
                    return v;
                }

                return b.Dictionary[key] = producer();
            }
        );
    }

    public void GetOrAddAndApply(TKey key, Func<TValue> producer, Action<TValue> apply)
    {
        ApplyUnlocked(
            key,
            (k, b) =>
            {
                using (b.Lock.EnterReadScope())
                {
                    if (b.Dictionary.TryGetValue(k, out var value))
                    {
                        apply(value);
                        return;
                    }
                }

                using var scope = b.Lock.EnterWriteScope();
                if (b.Dictionary.TryGetValue(key, out var v))
                {
                    apply(v);
                    return;
                }

                var result = b.Dictionary[key] = producer();
                apply(result);
            }
        );
    }

    public bool TryUpdate(TKey key, Func<TValue, TValue> updateFunc)
    {
        return Write(
            key,
            (k, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                d[k] = updateFunc(value);
                return true;
            }
        );
    }

    public bool ContainsKey(TKey key)
    {
        return Read(key, (k, d) => d.ContainsKey(k));
    }

    public void Add(TKey key, TValue value)
    {
        Write(key, (k, d) => d.Add(k, value));
    }

    public bool Remove(TKey key)
    {
        return Write(key, (k, d) => d.Remove(k));
    }

    public void Clear()
    {
        foreach (var bucket in _buckets)
        {
            using var scope = bucket.Lock.EnterWriteScope();
            bucket.Dictionary.Clear();
        }
    }
}
