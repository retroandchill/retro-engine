// // @file ViewModelForAttribute.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
#if !RETRO_ENGINE_EDITOR_SOURCE_GENERATOR
using Avalonia.Controls;
#endif

namespace RetroEngine.Editor.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ViewModelForAttribute<[UsedImplicitly] TView> : Attribute
#if !RETRO_ENGINE_EDITOR_SOURCE_GENERATOR
    where TView : Control, new();
#else
    where TView : class, new();
#endif
