// // @file ICustomCulture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

public interface ICustomCulture
{
    Culture BaseCulture { get; }
    string DisplayName { get; }
    string EnglishName { get; }
    string Name { get; }
    string NativeName { get; }
    string ThreeLetterISOLanguageName { get; }
    string TwoLetterISOLanguageName { get; }
    string Region { get; }
    string Script { get; }
    string Variant { get; }
    bool IsRightToLeft { get; }
}
