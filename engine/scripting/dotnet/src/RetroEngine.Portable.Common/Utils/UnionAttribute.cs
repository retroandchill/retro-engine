// // @file UnionAttribute.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Utils;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class UnionAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class UnionCaseAttribute : Attribute;

public interface IUnion;
