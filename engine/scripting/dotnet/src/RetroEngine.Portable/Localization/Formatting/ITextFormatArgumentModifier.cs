// // @file ITextFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Formatting;

public interface ITextFormatArgumentModifier
{
    (bool UsesFormatArgs, int Length) EstimateLength();

    void Evaluate<TContext>(FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct;
}
