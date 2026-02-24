// // @file TextingBinding.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Data;
using Avalonia.Markup.Xaml;
using RetroEngine.Editor.Core.Converters;

namespace RetroEngine.Editor.Core.Extensions;

public class TextBinding(string path) : MarkupExtension
{
    public string Path { get; } = path;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(Path) { Converter = new TextToStringConverter() };
    }
}
