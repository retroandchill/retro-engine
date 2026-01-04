// @file $BlittableTypeAttribute.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
public class BlittableTypeAttribute(string? cppName = null) : Attribute
{
    public string? CppName { get; } = cppName;

    public string? CppModule { get; init; } = null;
}
