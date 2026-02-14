// // @file ViewModelForInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Editor.Core.Attributes;

namespace RetroEngine.Editor.SourceGenerator.Model;

[AttributeInfoType(typeof(ViewModelForAttribute<>))]
public readonly record struct ViewModelForInfo(ITypeSymbol ViewType);
