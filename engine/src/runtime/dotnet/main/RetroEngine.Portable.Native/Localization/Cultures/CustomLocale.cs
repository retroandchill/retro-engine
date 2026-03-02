// // @file CustomLocale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

internal sealed class CustomLocale(ICustomCulture customCulture) : Locale(customCulture.BaseCulture.Name)
{
    public override string Name => customCulture.Name;
    public override string NativeName => customCulture.NativeName;
    public override string DisplayName => customCulture.DisplayName;
    public override string EnglishName => customCulture.EnglishName;
    public override string ThreeLetterISOLanguageName => customCulture.ThreeLetterISOLanguageName;
    public override string TwoLetterISOLanguageName => customCulture.TwoLetterISOLanguageName;
    public override string Script => customCulture.Script;
    public override string Variant => customCulture.Variant;
    public override string Region => customCulture.Region;
    public override bool IsRightToLeft => customCulture.IsRightToLeft;
}
