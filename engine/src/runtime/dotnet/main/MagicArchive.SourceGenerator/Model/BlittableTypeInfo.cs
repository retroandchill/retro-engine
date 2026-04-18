// // @file BlittableTypeInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.SourceGenerator.Model;

public enum BlittableType
{
    NotBlittable,
    BlittableSimple,
    BlittableComplex,
}

public readonly record struct BlittableTypeInfo(BlittableType Type, int Size, int Alignment)
{
    private const int NonBlittableValue = -1;

    public static readonly BlittableTypeInfo NotBlittable = new(
        BlittableType.NotBlittable,
        NonBlittableValue,
        NonBlittableValue
    );

    public static BlittableTypeInfo Simple(int size) => new(BlittableType.BlittableSimple, size, size);

    public bool IsBlittable => Type != BlittableType.NotBlittable;

    public bool IsSimple => Type == BlittableType.BlittableSimple;

    public bool IsComplex => Type == BlittableType.BlittableComplex;
}
