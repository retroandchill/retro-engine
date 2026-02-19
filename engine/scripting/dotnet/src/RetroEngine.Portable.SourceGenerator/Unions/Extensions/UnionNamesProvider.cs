// // @file UnionNamesProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.SourceGenerator.Unions.Extensions;

public static class UnionNamesProvider
{
    public static string GetIsCasePropertyName(string unionCaseName) => $"Is{unionCaseName}";

    public static string GetTryGetCaseDataMethodName(string unionCaseName) => $"TryGet{unionCaseName}Data";
}
