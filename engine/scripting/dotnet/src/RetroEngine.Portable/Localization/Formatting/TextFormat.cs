// // @file TextFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Formatting;

internal abstract record FormatSegment;

internal sealed record LiteralSegment(string Text) : FormatSegment;

internal sealed record PlaceholderSegment(string Key) : FormatSegment;

public sealed class TextFormat
{
    public string SourceString { get; }

    private readonly List<FormatSegment> _segments = [];

    private void Compile(string formatString)
    {
        var segments = new List<FormatSegment>();
        var currentText = new StringBuilder();
        var placeholder = new StringBuilder();

        var isEscaped = false;
        var inPlaceholder = false;

        foreach (var c in formatString)
        {
            if (isEscaped)
            {
                if (inPlaceholder)
                {
                    placeholder.Append(c);
                }
                else
                {
                    currentText.Append(c);
                }
            }

            if (c == '`')
            {
                isEscaped = true;
                continue;
            }

            if (!inPlaceholder)
            {
                if (c == '{')
                {
                    if (currentText.Length > 0)
                    {
                        segments.Add(new LiteralSegment(currentText.ToString()));
                        currentText.Clear();
                    }

                    inPlaceholder = true;
                }
                else
                {
                    currentText.Append(c);
                }
            }
            else
            {
                if (c == '}')
                {
                    var key = placeholder.ToString().Trim();
                    if (string.IsNullOrEmpty(key))
                        throw new FormatException("Invalid placeholder format.");

                    segments.Add(new PlaceholderSegment(key));
                    placeholder.Clear();
                    inPlaceholder = false;
                }
                else
                {
                    placeholder.Append(c);
                }
            }
        }

        if (isEscaped)
        {
            throw new FormatException("Unterminated escape sequence.");
        }

        if (inPlaceholder)
        {
            throw new FormatException("Unterminated placeholder.");
        }

        if (currentText.Length > 0)
            segments.Add(new LiteralSegment(currentText.ToString()));

        _segments.Clear();
        _segments.AddRange(segments);
    }
}
