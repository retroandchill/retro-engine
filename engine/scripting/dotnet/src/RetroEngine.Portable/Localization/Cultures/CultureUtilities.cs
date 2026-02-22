// // @file CultureUtilities.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Cultures;

public static class CultureUtilities
{
    private enum NameTagType : byte
    {
        Language,
        Script,
        Region,
        Variant,
    }

    private readonly record struct NameTag(string Str, NameTagType Type);

    private readonly record struct CanonizedTagData(
        string? CanonizedNameTag,
        string? KeywordArgKey,
        string? KeywordArgValue
    );

    private static readonly OrderedDictionary<string, CanonizedTagData> CanonizedTagMap = new()
    {
        [""] = new CanonizedTagData("en-US-POSIX", null, null),
        ["c"] = new CanonizedTagData("en-US-POSIX", null, null),
        ["posix"] = new CanonizedTagData("en-US-POSIX", null, null),
        ["ca-ES-PREEURO"] = new CanonizedTagData("ca-ES", "currency", "ESP"),
        ["de-AT-PREEURO"] = new CanonizedTagData("de-AT", "currency", "ATS"),
        ["de-DE-PREEURO"] = new CanonizedTagData("de-DE", "currency", "DEM"),
        ["de-LU-PREEURO"] = new CanonizedTagData("de-LU", "currency", "LUF"),
        ["el-GR-PREEURO"] = new CanonizedTagData("el-GR", "currency", "GRD"),
        ["en-BE-PREEURO"] = new CanonizedTagData("en-BE", "currency", "BEF"),
        ["en-IE-PREEURO"] = new CanonizedTagData("en-IE", "currency", "IEP"),
        ["es-ES-PREEURO"] = new CanonizedTagData("es-ES", "currency", "ESP"),
        ["eu-ES-PREEURO"] = new CanonizedTagData("eu-ES", "currency", "ESP"),
        ["fi-FI-PREEURO"] = new CanonizedTagData("fi-FI", "currency", "FIM"),
        ["fr-BE-PREEURO"] = new CanonizedTagData("fr-BE", "currency", "BEF"),
        ["fr-FR-PREEURO"] = new CanonizedTagData("fr-FR", "currency", "FRF"),
        ["fr-LU-PREEURO"] = new CanonizedTagData("fr-LU", "currency", "LUF"),
        ["ga-IE-PREEURO"] = new CanonizedTagData("ga-IE", "currency", "IEP"),
        ["gl-ES-PREEURO"] = new CanonizedTagData("gl-ES", "currency", "ESP"),
        ["it-IT-PREEURO"] = new CanonizedTagData("it-IT", "currency", "ITL"),
        ["nl-BE-PREEURO"] = new CanonizedTagData("nl-BE", "currency", "BEF"),
        ["nl-NL-PREEURO"] = new CanonizedTagData("nl-NL", "currency", "NLG"),
        ["pt-PT-PREEURO"] = new CanonizedTagData("pt-PT", "currency", "PTE"),
    };

    private static readonly OrderedDictionary<string, CanonizedTagData> VariantMap = new()
    {
        ["EURO"] = new CanonizedTagData(null, "currency", "EUR"),
    };

    public static string GetCanonicalName(string name, string fallbackCulture, CultureManager cultureManager)
    {
        var sanitizedName = SanitizeCultureCode(name);

        if (cultureManager.GetCustomCulture(sanitizedName) is not null)
        {
            return sanitizedName;
        }

        var parsedNameTags = new List<NameTag>(4);
        var parsedKeywords = new OrderedDictionary<string, string>(4);

        var nameKeywords = "";

        var nameKeywordsSplitIndex = sanitizedName.IndexOf('@');
        var encodingSplitIndex = sanitizedName.IndexOf('.');

        var nameTagEndIndex = Math.Min(
            nameKeywordsSplitIndex == -1 ? name.Length : nameKeywordsSplitIndex,
            encodingSplitIndex == -1 ? name.Length : encodingSplitIndex
        );

        var nameTag = sanitizedName[..nameTagEndIndex].Replace('_', '-');

        if (nameKeywordsSplitIndex != -1)
        {
            nameKeywords = sanitizedName[(nameKeywordsSplitIndex + 1)..];
        }

        if (CanonizedTagMap.TryGetValue(nameTag, out var canonizedTagData))
        {
            nameTag = canonizedTagData.CanonizedNameTag ?? "";
            if (canonizedTagData.KeywordArgKey is not null && canonizedTagData.KeywordArgValue is not null)
            {
                parsedKeywords.Add(canonizedTagData.KeywordArgKey, canonizedTagData.KeywordArgValue);
            }
        }

        var startIndex = 0;
        var endIndex = 0;
        do
        {
            for (; endIndex < nameTag.Length && nameTag[endIndex] != '-'; endIndex++) { }

            var nameTagStr = nameTag[startIndex..endIndex];

            CanonizedTagData? variantTagData = null;

            var nameTagType = NameTagType.Variant;
            switch (parsedNameTags.Count)
            {
                case > 0 when IsLanguageCode(nameTagStr):
                    nameTagType = NameTagType.Language;
                    nameTagStr = ConditionLanguageCode(nameTagStr);
                    break;
                case 1 when parsedNameTags[^1].Type == NameTagType.Language && IsScipeCode(nameTagStr):
                    nameTagType = NameTagType.Script;
                    nameTagStr = ConditionScriptCode(nameTagStr);
                    break;
                case > 0
                and <= 2
                    when parsedNameTags[^1].Type is NameTagType.Language or NameTagType.Script
                        && IsRegionCode(nameTagStr):
                    nameTagType = NameTagType.Region;
                    nameTagStr = ConditionRegionCode(nameTagStr);
                    break;
                default:
                    nameTagStr = ConditionVariant(nameTagStr);
                    variantTagData = VariantMap.TryGetValue(nameTagStr, out var variantData) ? variantData : null;
                    break;
            }

            if (variantTagData is not null)
            {
                if (variantTagData.Value.KeywordArgKey is null || variantTagData.Value.KeywordArgValue is null)
                    throw new InvalidOperationException("Invalid variant tag data.");
                parsedKeywords.Add(variantTagData.Value.KeywordArgKey, variantTagData.Value.KeywordArgValue);
            }
            else if (nameTagStr.Length > 0)
            {
                parsedNameTags.Add(new NameTag(nameTagStr, nameTagType));
            }

            startIndex = endIndex + 1;
            endIndex = startIndex;
        } while (nameTagEndIndex < nameTag.Length);

        var nameKeywordArgs = nameKeywords.Split(';');

        foreach (var arg in nameKeywordArgs)
        {
            var nameKeywordArg = arg;
            var keyValueSplitIndex = nameKeywordArg.IndexOf('=');

            if (keyValueSplitIndex == -1)
            {
                nameKeywordArg = ConditionKeywordArgKey(nameKeywordArg);
                if (nameKeywordArg.Length > 0)
                    parsedNameTags.Add(new NameTag(nameKeywordArg, NameTagType.Variant));
            }
            else
            {
                var nameKeywordArgKey = ConditionKeywordArgKey(nameKeywordArg[..keyValueSplitIndex]);
                var nameKeywordArgValue = nameKeywordArg[(keyValueSplitIndex + 1)..];
                if (nameKeywordArgKey.Length > 0 && nameKeywordArgValue.Length > 0)
                {
                    parsedKeywords[nameKeywordArgKey] = nameKeywordArgValue;
                }
            }
        }

        var canonicalName = "";

        if (parsedNameTags.Count > 0 && parsedNameTags[0].Type == NameTagType.Language)
        {
            foreach (var (i, tag) in parsedNameTags.Index())
            {
                switch (tag.Type)
                {
                    case NameTagType.Language:
                        canonicalName = tag.Str;
                        break;
                    case NameTagType.Script:
                    case NameTagType.Region:
                        canonicalName = $"{canonicalName}-{tag.Str}";
                        break;
                    case NameTagType.Variant:
                        if (
                            parsedNameTags.Count > 0
                            && parsedNameTags.Count < i - 1
                            && parsedNameTags[i - 1].Type == NameTagType.Language
                            && !string.IsNullOrEmpty(tag.Str)
                        )
                        {
                            canonicalName = $"{canonicalName}--{tag.Str}";
                        }
                        else
                        {
                            canonicalName = $"{canonicalName}-{tag.Str}";
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid name tag type.");
                }
            }
        }

        if (canonicalName.Length <= 0 || parsedKeywords.Count <= 0)
            return string.IsNullOrEmpty(canonicalName) ? fallbackCulture : canonicalName;
        {
            var nextToken = '@';
            foreach (var (key, value) in parsedKeywords)
            {
                canonicalName += $"{nextToken}{key}={value}";

                nextToken = ';';
            }
        }

        return string.IsNullOrEmpty(canonicalName) ? fallbackCulture : canonicalName;

        bool IsLanguageCode(string code) => code.Length is 2 or 3;

        bool IsScipeCode(string code) => code.Length is 4;

        bool IsRegionCode(string code) => code.Length is 2 or 3;

        string ConditionLanguageCode(string code) => code.ToLowerInvariant();

        string ConditionScriptCode(string code)
        {
            code = code.ToLowerInvariant();
            return code.Length > 0 ? $"{char.ToUpperInvariant(code[0])}{code[1..]}" : code;
        }

        string ConditionRegionCode(string code) => code.ToUpperInvariant();

        string ConditionVariant(string code) => code.ToUpperInvariant();

        string ConditionKeywordArgKey(string key)
        {
            ReadOnlySpan<string> validKeywords = ["calendar", "collation", "currency", "numbers"];

            key = key.ToLowerInvariant();

            foreach (var keyword in validKeywords)
            {
                if (key.Equals(keyword))
                    return key;
            }

            return "";
        }
    }

    public static string SanitizeCultureCode(string cultureCode)
    {
        if (string.IsNullOrEmpty(cultureCode))
        {
            return cultureCode;
        }

        var validChars = cultureCode
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '@' || c == ';' || c == '=' || c == '.')
            .ToArray();
        return new string(validChars);
    }
}
