// // @file Result.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using LinkDotNet.StringBuilder;
using ZParse.Display;

namespace ZParse;

/// <summary>
/// Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="T">The type of the result</typeparam>
public readonly ref struct Result<T>
    where T : allows ref struct
{
    /// <summary>
    /// The view that was originally parsed.
    /// </summary>
    public StringView Input { get; }

    /// <summary>
    /// The remainder of the input after parsing.
    /// </summary>
    public StringView Remainder { get; }

    /// <summary>
    /// If the parsing operation was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// The position of the error, if any.
    /// </summary>
    public TextPosition ErrorPosition => Success ? TextPosition.Empty : Input.Position;

    /// <summary>
    /// List of expectations that were not met.
    /// </summary>
    public ImmutableArray<string> Expectations { get; }

    internal bool IsPartial(StringView from) => from != Remainder;

    internal bool Backtrack { get; init; }

    /// <summary>
    /// The parsed value.
    /// </summary>
    public T Value => Success ? field : throw new InvalidOperationException($"{nameof(Result<>)} has no value.");

    internal Result(T value, StringView input, StringView remainder, bool backtrack)
    {
        Input = input;
        Remainder = remainder;
        Value = value;
        Success = true;
        Expectations = default;
        Backtrack = backtrack;
    }

    internal Result(StringView input, StringView remainder, ImmutableArray<string> expectations, bool backtrack)
    {
        Input = input;
        Remainder = remainder;
        Success = false;
        Value = default!;
        Expectations = expectations;
        Backtrack = backtrack;
    }

    internal Result(StringView remainder, ImmutableArray<string> expectations, bool backtrack)
        : this(remainder, remainder, expectations, backtrack) { }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Remainder == StringView.None)
            return "(Empty result.)";

        if (Success)
            return "Successful parsing";

        var builder = new ValueStringBuilder();
        try
        {
            builder.Append("Syntax error");
            if (!Input.IsAtEnd)
            {
                builder.Append(" (line ");
                builder.Append(Input.Position.Line);
                builder.Append(", column ");
                builder.Append(Input.Position.Column);
                builder.Append(")");
            }

            builder.Append(": ");
            AppendErrorMessage(ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    /// <summary>
    /// Get the fully formatted error message for the result.
    /// </summary>
    /// <returns>The error message for the result.</returns>
    public string GetErrorMessage()
    {
        var builder = new ValueStringBuilder();
        try
        {
            AppendErrorMessage(ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private void AppendErrorMessage(ref ValueStringBuilder builder)
    {
        if (Input.IsAtEnd)
        {
            builder.Append("unexpected end of input");
        }
        else
        {
            var next = Input.TryGetNext().Value;
            builder.Append("unexpected ");
            builder.AppendLiteral(next);
        }

        if (Expectations.IsDefaultOrEmpty)
            return;

        builder.Append(", expected ");
        builder.AppendFriendlyList(Expectations);
        builder.Append('.');
    }
}

/// <summary>
/// Helper methods for creating <see cref="Result{T}"/> instances.
/// </summary>
public static class Result
{
    /// <summary>
    /// Create an empty result indicating no value was able to be parsed.
    /// </summary>
    /// <param name="remainder">The start of the text that was unable to be parsed.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>An empty result.</returns>
    public static Result<T> Empty<T>(StringView remainder)
        where T : allows ref struct
    {
        return new Result<T>(remainder, ImmutableArray<string>.Empty, false);
    }

    /// <summary>
    /// Create an empty result indicating no value was able to be parsed.
    /// </summary>
    /// <param name="remainder">The start of the text that was unable to be parsed.</param>
    /// <param name="expectations">The expectations that were not met.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>An empty result.</returns>
    public static Result<T> Empty<T>(StringView remainder, ImmutableArray<string> expectations)
        where T : allows ref struct
    {
        return new Result<T>(remainder, expectations, false);
    }

    /// <summary>
    /// Create a successful result with the given value.
    /// </summary>
    /// <param name="value">The value of the result</param>
    /// <param name="input">The input that was ingested for the parse</param>
    /// <param name="remainder">The start of the remaining unparsed text</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>The created result.</returns>
    public static Result<T> Value<T>(T value, StringView input, StringView remainder)
        where T : allows ref struct
    {
        return new Result<T>(value, input, remainder, false);
    }

    /// <summary>
    /// Convert an empty result to another type.
    /// </summary>
    /// <param name="result">The result to convert</param>
    /// <typeparam name="TOther">The target type</typeparam>
    /// <typeparam name="T">The source type</typeparam>
    /// <returns>A result of type <typeparamref name="TOther"/> carrying the same information as <paramref name="result"/>.</returns>
    public static Result<TOther> CastEmpty<T, TOther>(Result<T> result)
        where T : allows ref struct
        where TOther : allows ref struct
    {
        return new Result<TOther>(result.Remainder, result.Expectations, result.Backtrack);
    }

    /// <summary>
    /// Combine two empty results.
    /// </summary>
    /// <param name="other">The other result to combine with</param>
    /// <param name="result">The result to convert</param>
    /// <typeparam name="T">The source type</typeparam>
    /// <returns>A result of type <typeparamref name="T"/> carrying information from both results.</returns>
    public static Result<T> CombineEmpty<T>(Result<T> result, Result<T> other)
        where T : allows ref struct
    {
        if (result.Remainder != other.Remainder)
            return other;

        var expectations = result.Expectations;
        if (expectations.IsDefaultOrEmpty)
            expectations = other.Expectations;
        else if (!other.Expectations.IsDefaultOrEmpty)
            expectations = expectations.AddRange(other.Expectations);

        return new Result<T>(other.Remainder, expectations, other.Backtrack);
    }
}
