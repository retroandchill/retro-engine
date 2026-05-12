// // @file StrippedDictionary.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RetroEngine.Utilities.Concurrency;

namespace RetroEngine.Utilities.Collections;

internal readonly ref struct StripedDictionaryContext<TContext, TValue>(TContext context, TValue value)
    where TContext : allows ref struct
{
    public TContext Context { get; } = context;

    public TValue Value { get; } = value;

    public void Deconstruct(out TContext context, out TValue value)
    {
        context = Context;
        value = Value;
    }
}

internal static class StripedDictionaryContext
{
    public static StripedDictionaryContext<TContext, TValue> Create<TContext, TValue>(TContext context, TValue value)
        where TContext : allows ref struct
    {
        return new StripedDictionaryContext<TContext, TValue>(context, value);
    }
}

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

    private void Write<TContext>(TContext context, TKey key, Action<TKey, TContext, Dictionary<TKey, TValue>> func)
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterWriteScope();
        func(key, context, bucket.Dictionary);
    }

    private TResult Write<TResult, TContext>(
        TContext context,
        TKey key,
        Func<TKey, TContext, Dictionary<TKey, TValue>, TResult> func
    )
    {
        var bucket = _buckets[GetStripeIndex(key)];
        using var scope = bucket.Lock.EnterWriteScope();
        return func(key, context, bucket.Dictionary);
    }

    private void ApplyUnlocked<TContext>(TKey key, TContext context, Action<TKey, TContext, Bucket> func)
        where TContext : allows ref struct
    {
        var bucket = _buckets[GetStripeIndex(key)];
        func(key, context, bucket);
    }

    private TResult ApplyUnlocked<TResult, TContext>(
        TKey key,
        TContext context,
        Func<TKey, TContext, Bucket, TResult> func
    )
        where TContext : allows ref struct
    {
        var bucket = _buckets[GetStripeIndex(key)];
        return func(key, context, bucket);
    }

    public TValue this[TKey key]
    {
        get { return Read(key, static (k, d) => d[k]); }
        set { Write(value, key, static (k, v, d) => d[k] = v); }
    }

    public TValue? GetValueOrDefault(TKey key)
    {
        return Read(key, (k, d) => d.GetValueOrDefault(k));
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        (var success, value) = Read(key, static (k, d) => d.TryGetValue(k, out var v) ? (true, v) : (false, default));
        return success;
    }

    public bool GetAndApply(TKey key, Action<TValue> action)
    {
        return Read(
            action,
            key,
            static (a, k, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                a(value);
                return true;
            }
        );
    }

    public bool GetAndApply<TContext>(TKey key, TContext context, Action<TValue, TContext> action)
        where TContext : allows ref struct
    {
        return Read(
            StripedDictionaryContext.Create(context, action),
            key,
            static (ctx, k, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                ctx.Value(value, ctx.Context);
                return true;
            }
        );
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> producer)
    {
        return ApplyUnlocked(
            key,
            producer,
            static (k, p, b) =>
            {
                using (b.Lock.EnterReadScope())
                {
                    if (b.Dictionary.TryGetValue(k, out var value))
                        return value;
                }

                using var scope = b.Lock.EnterWriteScope();
                if (b.Dictionary.TryGetValue(k, out var v))
                {
                    return v;
                }

                return b.Dictionary[k] = p(k);
            }
        );
    }

    public TValue GetOrAdd<TContext>(TKey key, TContext context, Func<TKey, TContext, TValue> producer)
        where TContext : allows ref struct
    {
        return ApplyUnlocked(
            key,
            StripedDictionaryContext.Create(context, producer),
            static (k, p, b) =>
            {
                using (b.Lock.EnterReadScope())
                {
                    if (b.Dictionary.TryGetValue(k, out var value))
                        return value;
                }

                using var scope = b.Lock.EnterWriteScope();
                if (b.Dictionary.TryGetValue(k, out var v))
                {
                    return v;
                }

                return b.Dictionary[k] = p.Value(k, p.Context);
            }
        );
    }

    public bool TryGetOrRemove(TKey key, [NotNullWhen(true)] out TValue? value, Func<TValue, bool> predicate)
    {
        (var success, value) = ApplyUnlocked(
            key,
            predicate,
            static (k, p, b) =>
            {
                using var scope = b.Lock.EnterUpgradeableReadScope();
                if (!b.Dictionary.TryGetValue(k, out var v))
                    return (false, default);

                if (p(v))
                    return (true, v);

                using var writeScope = b.Lock.EnterWriteScope();
                b.Dictionary.Remove(k);
                return (false, default);
            }
        );
        return success;
    }

    public void GetOrAddAndApply(TKey key, Func<TValue> producer, Action<TValue> apply)
    {
        ApplyUnlocked(
            key,
            (apply, producer),
            static (k, a, b) =>
            {
                using (b.Lock.EnterReadScope())
                {
                    if (b.Dictionary.TryGetValue(k, out var value))
                    {
                        a.apply(value);
                        return;
                    }
                }

                using var scope = b.Lock.EnterWriteScope();
                if (b.Dictionary.TryGetValue(k, out var v))
                {
                    a.apply(v);
                    return;
                }

                var result = b.Dictionary[k] = a.producer();
                a.apply(result);
            }
        );
    }

    public bool TryUpdate(TKey key, Func<TValue, TValue> updateFunc)
    {
        return Write(
            updateFunc,
            key,
            static (k, u, d) =>
            {
                if (!d.TryGetValue(k, out var value))
                    return false;

                d[k] = u(value);
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
        Write(value, key, static (k, v, d) => d.Add(k, v));
    }

    public bool ChangeKey(TKey oldKey, TKey newKey, [NotNullWhen(true)] out TValue? value)
    {
        if (_comparer.Equals(oldKey, newKey))
        {
            return TryGetValue(newKey, out value);
        }

        var oldBucketIndex = GetStripeIndex(oldKey);
        var newBucketIndex = GetStripeIndex(newKey);
        if (oldBucketIndex == newBucketIndex)
        {
            var (dict, @lock) = _buckets[oldBucketIndex];
            using var scope = @lock.EnterWriteScope();
            if (!dict.Remove(oldKey, out value!))
                return false;
            try
            {
                dict.Add(newKey, value);
            }
            catch
            {
                dict.Add(oldKey, value);
                throw;
            }

            return true;
        }

        var (oldDict, oldLock) = _buckets[oldBucketIndex];
        using var oldScope = oldLock.EnterWriteScope();
        if (!oldDict.Remove(oldKey, out value))
            return false;
        var (newDict, newLock) = _buckets[newBucketIndex];
        using var newScope = newLock.EnterWriteScope();

        try
        {
            newDict.Add(newKey, value);
        }
        catch
        {
            oldDict.Add(oldKey, value);
            throw;
        }

        return true;
    }

    public bool Remove(TKey key)
    {
        return Write(0, key, static (k, _, d) => d.Remove(k));
    }

    public bool Remove(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        (var removed, value) = Write(0, key, static (k, _, d) => d.Remove(k, out var v) ? (true, v) : (false, v));

        return removed;
    }

    public void RemoveIf(Func<TKey, TValue, bool> predicate)
    {
        foreach (var buck in _buckets)
        {
            using var scope = buck.Lock.EnterWriteScope();
            using var fixedList = new FixedList<TKey>(buck.Dictionary.Count);
            foreach (var (key, value) in buck.Dictionary)
            {
                if (predicate(key, value))
                {
                    fixedList.Add(key);
                }
            }

            foreach (var key in fixedList)
            {
                buck.Dictionary.Remove(key);
            }
        }
    }

    public void Clear()
    {
        foreach (var bucket in _buckets)
        {
            using var scope = bucket.Lock.EnterWriteScope();
            bucket.Dictionary.Clear();
        }
    }

    public void ForEach(Action<TKey, TValue> action)
    {
        foreach (var buck in _buckets)
        {
            using var scope = buck.Lock.EnterReadScope();
            foreach (var (key, value) in buck.Dictionary)
            {
                action(key, value);
            }
        }
    }
}
