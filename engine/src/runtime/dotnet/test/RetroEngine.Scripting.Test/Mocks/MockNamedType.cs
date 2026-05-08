// // @file MockNamedType.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Scripting.Model;

namespace RetroEngine.Scripting.Test.Mocks;

public sealed record MockNamedType(string Namespace, string Name) : INamedScriptType
{
    public string FullName => $"{Namespace}.{Name}";
    public string FullCodeName => FullName;
    public string FullCodeNameUnbound => FullName;
}
