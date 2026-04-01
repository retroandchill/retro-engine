// // @file Presentation.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;

namespace ZParse.Display;

internal static class Presentation
{
    extension(ref ValueStringBuilder builder)
    {
        public void AppendLiteral(char literal)
        {
            switch (literal)
            {
                //Unicode Category: Space Separators
                case '\x00A0':
                    builder.Append("U+00A0 no-break space");
                    break;
                case '\x1680':
                    builder.Append("U+1680 ogham space mark");
                    break;
                case '\x2000':
                    builder.Append("U+2000 en quad");
                    break;
                case '\x2001':
                    builder.Append("U+2001 em quad");
                    break;
                case '\x2002':
                    builder.Append("U+2002 en space");
                    break;
                case '\x2003':
                    builder.Append("U+2003 em space");
                    break;
                case '\x2004':
                    builder.Append("U+2004 three-per-em space");
                    break;
                case '\x2005':
                    builder.Append("U+2005 four-per-em space");
                    break;
                case '\x2006':
                    builder.Append("U+2006 six-per-em space");
                    break;
                case '\x2007':
                    builder.Append("U+2007 figure space");
                    break;
                case '\x2008':
                    builder.Append("U+2008 punctuation space");
                    break;
                case '\x2009':
                    builder.Append("U+2009 thin space");
                    break;
                case '\x200A':
                    builder.Append("U+200A hair space");
                    break;
                case '\x202F':
                    builder.Append("U+202F narrow no-break space");
                    break;
                case '\x205F':
                    builder.Append("U+205F medium mathematical space");
                    break;
                case '\x3000':
                    builder.Append("U+3000 ideographic space");
                    break;

                //Line Separator
                case '\x2028':
                    builder.Append("U+2028 line separator");
                    break;

                //Paragraph Separator
                case '\x2029':
                    builder.Append("U+2029 paragraph separator");
                    break;

                //Unicode C0 Control Codes (ASCII equivalent)
                case '\x0000': //\0
                    builder.Append("NUL");
                    break;
                case '\x0001':
                    builder.Append("U+0001 start of heading");
                    break;
                case '\x0002':
                    builder.Append("U+0002 start of text");
                    break;
                case '\x0003':
                    builder.Append("U+0003 end of text");
                    break;
                case '\x0004':
                    builder.Append("U+0004 end of transmission");
                    break;
                case '\x0005':
                    builder.Append("U+0005 enquiry");
                    break;
                case '\x0006':
                    builder.Append("U+0006 acknowledge");
                    break;
                case '\x0007':
                    builder.Append("U+0007 bell");
                    break;
                case '\x0008':
                    builder.Append("U+0008 backspace");
                    break;
                case '\x0009': //\t
                    builder.Append("tab");
                    break;
                case '\x000A': //\n
                    builder.Append("line feed");
                    break;
                case '\x000B':
                    builder.Append("U+000B vertical tab");
                    break;
                case '\x000C':
                    builder.Append("U+000C form feed");
                    break;
                case '\x000D': //\r
                    builder.Append("carriage return");
                    break;
                case '\x000E':
                    builder.Append("U+000E shift in");
                    break;
                case '\x000F':
                    builder.Append("U+000F shift out");
                    break;
                case '\x0010':
                    builder.Append("U+0010 data link escape");
                    break;
                case '\x0011':
                    builder.Append("U+0011 device ctrl 1");
                    break;
                case '\x0012':
                    builder.Append("U+0012 device ctrl 2");
                    break;
                case '\x0013':
                    builder.Append("U+0013 device ctrl 3");
                    break;
                case '\x0014':
                    builder.Append("U+0014 device ctrl 4");
                    break;
                case '\x0015':
                    builder.Append("U+0015 not acknowledge");
                    break;
                case '\x0016':
                    builder.Append("U+0016 synchronous idle");
                    break;
                case '\x0017':
                    builder.Append("U+0017 end transmission block");
                    break;
                case '\x0018':
                    builder.Append("U+0018 cancel");
                    break;
                case '\x0019':
                    builder.Append("U+0019 end of medium");
                    break;
                case '\x0020':
                    builder.Append("space");
                    break;
                case '\x001A':
                    builder.Append("U+001A substitute");
                    break;
                case '\x001B':
                    builder.Append("U+001B escape");
                    break;
                case '\x001C':
                    builder.Append("U+001C file separator");
                    break;
                case '\x001D':
                    builder.Append("U+001D group separator");
                    break;
                case '\x001E':
                    builder.Append("U+001E record separator");
                    break;
                case '\x001F':
                    builder.Append("U+001F unit separator");
                    break;
                case '\x007F':
                    builder.Append("U+007F delete");
                    break;

                default:
                    builder.Append('`');
                    builder.Append(literal);
                    builder.Append('`');
                    break;
            }
        }

        public void AppendLiteral(ReadOnlySpan<char> literal)
        {
            builder.Append('`');
            builder.Append(literal);
            builder.Append('`');
        }
    }

    public static string FormatLiteral(char literal)
    {
        var builder = new ValueStringBuilder();
        try
        {
            builder.AppendLiteral(literal);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    public static string FormatLiteral(ReadOnlySpan<char> literal)
    {
        var builder = new ValueStringBuilder();
        try
        {
            builder.AppendLiteral(literal);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }
}
