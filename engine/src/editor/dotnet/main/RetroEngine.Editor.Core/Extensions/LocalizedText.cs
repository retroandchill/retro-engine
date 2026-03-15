// // @file LocalizedText.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Markup.Xaml;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Extensions;

public sealed class LocalizedText(string ns, string key, string sourceString) : MarkupExtension
{
    public TextKey Namespace { get; } = ns;
    public TextKey Key { get; } = key;
    public string SourceString { get; } = sourceString;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var text = Text.AsLocalizable(Namespace, Key, SourceString);
        return text.ToString();
    }
}
