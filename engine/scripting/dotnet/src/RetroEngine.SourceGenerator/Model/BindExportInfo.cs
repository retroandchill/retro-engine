// @file $BindExportInfo.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Binds;

namespace RetroEngine.SourceGenerator.Model;

[AttributeInfoType<BindExportAttribute>]
public readonly record struct BindExportInfo(string? CppNamespace);
