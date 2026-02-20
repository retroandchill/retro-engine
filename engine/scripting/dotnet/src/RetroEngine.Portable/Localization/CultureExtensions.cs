// // @file CultureExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ICU4N.Globalization;

namespace RetroEngine.Portable.Localization;

internal static class CultureExtensions
{
    private const string LANG_DIR_STRING = "root-en-es-pt-zh-ja-ko-de-fr-it-ar+he+fa+ru-nl-pl-th-tr-";

    extension(UCultureInfo ci)
    {
        public bool IsRightToLeft
        {
            get
            {
                var script = ci.Script;
                if (script.Length == 0)
                {
                    // Fastpath: We know the likely scripts and their writing direction
                    // for some common languages.
                    var lang = ci.Language;
                    if (lang.Length == 0)
                    {
                        return false;
                    }
                    var langIndex = LANG_DIR_STRING.IndexOf(lang, StringComparison.Ordinal);
                    if (langIndex >= 0)
                    {
                        switch (LANG_DIR_STRING[langIndex + lang.Length])
                        {
                            case '-':
                                return false;
                            case '+':
                                return true;
                        }
                    }
                    // Otherwise, find the likely script.
                    var likely = UCultureInfo.AddLikelySubtags(ci);
                    script = likely.Script;
                    if (script.Length == 0)
                    {
                        return false;
                    }
                }
                var scriptCode = UScript.GetCodeFromName(script);
                return UScript.IsRightToLeft(scriptCode);
            }
        }
    }
}
