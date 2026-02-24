// // @file TextToStringConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using Avalonia.Data.Converters;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Converters;

public class TextToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Text text && targetType == typeof(string) ? text.ToString() : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text && targetType == typeof(Text) ? new Text(text) : value;
    }
}
