// @file $CppTypeAttribute.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class CppTypeAttribute : Attribute
{
    public string? TypeName { get; init; }

    public string? CppModule { get; init; }

    public bool UseReference { get; init; }

    public bool IsConst { get; init; }
}
