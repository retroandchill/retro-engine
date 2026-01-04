// @file $BindExportAttribute.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BindExportAttribute(string? cppNamespace = null) : Attribute
{
    public string? CppNamespace { get; } = cppNamespace;
}
