// // @file TypeReference.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;

namespace RetroEngine.Scripting.Model;

public abstract record ScriptTypeReference;

public sealed record ClrTypeReference(string AssemblyName, string TypeName) : ScriptTypeReference;

public sealed record AssetTypeReference(AssetPath Path) : ScriptTypeReference;
