// // @file TextStringHelper.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using Superpower.Model;

namespace RetroEngine.Portable.Localization;

public static class TextStringHelper
{
    public static Result<Text> ReadFromBuffer(string buffer, string? textNamespace = null, bool requiresQuotes = false)
    {
        return ReadFromBuffer(new TextSpan(buffer), textNamespace, requiresQuotes);
    }

    internal static Result<Text> ReadFromBuffer(
        TextSpan buffer,
        string? textNamespace = null,
        bool requiresQuotes = false
    )
    {
        throw new NotImplementedException();
    }

    public static void WriteToBuffer(StringBuilder buffer, Text value, bool requiresQuotes = false)
    {
        throw new NotImplementedException();
    }
}
