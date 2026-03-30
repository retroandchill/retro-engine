// // @file TokenSelectEnumerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZLinq;

namespace ZParse.Enumeration;

public ref struct TokenSelectEnumerator<TFrom, TTo> : IEnumerator<TTo>, IValueEnumerator<TTo>
{
    private TokenEnumerator<TFrom> _source;
    private readonly Func<Token<TFrom>, TTo> _selector;

    internal TokenSelectEnumerator(TokenEnumerator<TFrom> source, Func<Token<TFrom>, TTo> selector)
    {
        _source = source;
        _selector = selector;
        Current = default!;
    }

    object? IEnumerator.Current => Current;
    public TTo Current { get; private set; }

    public TokenSelectEnumerator<TFrom, TTo> GetEnumerator() => this;

    public bool MoveNext()
    {
        if (!_source.MoveNext())
            return false;

        Current = _selector(_source.Current);
        return true;
    }

    public bool TryGetNext(out TTo current)
    {
        if (!_source.MoveNext())
        {
            current = default!;
            return false;
        }

        current = _selector(_source.Current);
        return true;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
    }

    public bool TryGetSpan(out ReadOnlySpan<TTo> span)
    {
        span = default;
        return false;
    }

    public bool TryCopyTo(scoped Span<TTo> destination, Index offset)
    {
        return false;
    }

    public void Reset()
    {
        _source.Reset();
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}

public static class TokenSelectEnumeratorExtensions
{
    public static ValueEnumerable<TokenSelectEnumerator<TFrom, TTo>, TTo> Select<TFrom, TTo>(
        this TokenEnumerator<TFrom> source,
        Func<Token<TFrom>, TTo> selector
    )
    {
        return new ValueEnumerable<TokenSelectEnumerator<TFrom, TTo>, TTo>(
            new TokenSelectEnumerator<TFrom, TTo>(source, selector)
        );
    }
}
