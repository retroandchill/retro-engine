// // @file ParseResult.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using LinkDotNet.StringBuilder;
using ZParse.Display;

namespace ZParse;

public readonly struct Unit
{
    public static Unit Value { get; } = new();
}

public delegate string GetParseExpectations(TextSegment input, TextSegment remainder);

public readonly struct ParseExpectation
{
    public object Value { get; }

    public ParseExpectation(string value)
    {
        Value = value;
    }

    public ParseExpectation(GetParseExpectations value)
    {
        Value = value;
    }

    public static implicit operator ParseExpectation(string value) => new(value);

    public static implicit operator ParseExpectation(GetParseExpectations value) => new(value);
}

public readonly ref struct ParseResult<T>
    where T : allows ref struct
{
    public bool HasValue { get; }

    public T Value => HasValue ? field : throw new InvalidOperationException("No value available.");

    public TextSegment Input { get; }

    public TextSegment Remainder { get; }

    public TextPosition ErrorPosition => HasValue ? TextPosition.Empty : Remainder.Position;

    internal string? ErrorMessage { get; }

    internal ImmutableArray<ParseExpectation> Expectations { get; }

    public bool IsPartial(TextSegment from) => from != Remainder;

    public bool Backtrack { get; }

    internal ParseResult(T value, TextSegment input, TextSegment remainder, bool backtrack)
    {
        HasValue = true;
        Value = value;
        Input = input;
        Remainder = remainder;
        ErrorMessage = null;
        Expectations = [];
        Backtrack = backtrack;
    }

    internal ParseResult(TextSegment remainder, string errorMessage, bool backtrack)
        : this(remainder, remainder, errorMessage, backtrack) { }

    internal ParseResult(TextSegment remainder, ImmutableArray<ParseExpectation> expectations, bool backtrack)
        : this(remainder, remainder, expectations, backtrack) { }

    internal ParseResult(TextSegment input, TextSegment remainder, string errorMessage, bool backtrack)
    {
        HasValue = false;
        Value = default!;
        Input = input;
        Remainder = remainder;
        ErrorMessage = errorMessage;
        Expectations = [];
        Backtrack = backtrack;
    }

    internal ParseResult(
        TextSegment input,
        TextSegment remainder,
        ImmutableArray<ParseExpectation> expectations,
        bool backtrack
    )
    {
        HasValue = false;
        Value = default!;
        Input = input;
        Remainder = remainder;
        ErrorMessage = null;
        Expectations = expectations;
        Backtrack = backtrack;
    }

    public override string ToString()
    {
        if (Remainder == TextSegment.None)
            return "(Empty result.)";

        if (HasValue)
            return "Successful parse";

        var builder = new ValueStringBuilder();
        try
        {
            builder.Append("Syntax error");
            if (!Remainder.IsAtEnd)
            {
                builder.Append(" (line ");
                builder.Append(Remainder.Position.Line);
                builder.Append(", column ");
                builder.Append(Remainder.Position.Column);
                builder.Append(")");
            }

            builder.Append(": ");
            FormatErrorMessageFragment(ref builder);
            builder.Append('.');
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    public string FormatErrorMessageFragment()
    {
        var builder = new ValueStringBuilder();
        try
        {
            FormatErrorMessageFragment(ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private void FormatErrorMessageFragment(ref ValueStringBuilder builder)
    {
        if (Input.IsAtEnd)
        {
            builder.Append("unexpected end of input");
        }
        else
        {
            var next = Input.ConsumeChar().Value;
            builder.Append("unexpected ");
            builder.AppendLiteral(next);
        }

        if (Expectations.IsDefaultOrEmpty)
            return;
        builder.Append(", expected ");
        var expectationStrings = new List<string>(Expectations.Length);
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var expectation in Expectations)
        {
            switch (expectation.Value)
            {
                case string str:
                    expectationStrings.Add(str);
                    continue;
                case GetParseExpectations getExpectation:
                    expectationStrings.Add(getExpectation(Input, Remainder));
                    continue;
            }
        }
        builder.AppendFriendlyList(CollectionsMarshal.AsSpan(expectationStrings));
    }
}

public static class ParseResult
{
    public static ParseResult<T> Success<T>(T value, TextSegment input, TextSegment remainder)
        where T : allows ref struct
    {
        return new ParseResult<T>(value, input, remainder, false);
    }

    public static ParseResult<T> Empty<T>(TextSegment remainder)
        where T : allows ref struct
    {
        return new ParseResult<T>(remainder, [], false);
    }

    public static ParseResult<T> Empty<T>(TextSegment remainder, ImmutableArray<ParseExpectation> expectations)
        where T : allows ref struct
    {
        return new ParseResult<T>(remainder, expectations, false);
    }

    public static ParseResult<T> Empty<T>(TextSegment remainder, string errorMessage)
        where T : allows ref struct
    {
        return new ParseResult<T>(remainder, errorMessage, false);
    }

    public static ParseResult<T> Empty<T>(TextSegment input, TextSegment remainder)
        where T : allows ref struct
    {
        return new ParseResult<T>(input, remainder, [], false);
    }

    public static ParseResult<T> Empty<T>(TextSegment input, TextSegment remainder, string errorMessage)
        where T : allows ref struct
    {
        return new ParseResult<T>(input, remainder, errorMessage, false);
    }

    public static ParseResult<T> Empty<T>(
        TextSegment input,
        TextSegment remainder,
        ImmutableArray<ParseExpectation> expectations
    )
        where T : allows ref struct
    {
        return new ParseResult<T>(input, remainder, expectations, false);
    }

    public static ParseResult<TOther> CastEmpty<T, TOther>(ParseResult<T> result)
        where T : allows ref struct
        where TOther : allows ref struct
    {
        return new ParseResult<TOther>(result.Input, result.Remainder, result.Expectations, result.Backtrack);
    }
}
