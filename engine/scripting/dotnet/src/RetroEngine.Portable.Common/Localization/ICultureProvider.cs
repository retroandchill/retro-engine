// // @file ICultureProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace RetroEngine.Portable.Localization;

public interface ICultureProvider
{
    CultureInfo CurrentCulture { get; }
    event Action<CultureInfo>? CurrentCultureChanged;

    bool TrySetCurrentCulture(CultureInfo culture);

    string CurrentNativeCultureName { get; }
    bool TrySetNativeCultureName(string cultureName);

    IEnumerable<CultureInfo> GetAvailableCultures();
    IEnumerable<string> GetAvailableNativeCultureNames();
}
