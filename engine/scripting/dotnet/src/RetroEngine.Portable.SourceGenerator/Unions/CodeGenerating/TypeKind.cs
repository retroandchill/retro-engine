// // @file TypeKind.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct BlittableTypeKindData
{
    [field: FieldOffset(0)]
    public (bool IsAbstract, bool IsSealed) ClassCase { get; init; }

    [field: FieldOffset(0)]
    public bool IsReadOnly { get; init; }
}

[Union]
public struct TypeKind
{
    private byte Index { get; init; }
    private BlittableTypeKindData Data { get; init; }

    [UnionCase]
    public static TypeKind Class(bool isAbstract, bool isSealed)
    {
        return new TypeKind
        {
            Index = 0,
            Data = new BlittableTypeKindData { ClassCase = (isAbstract, isSealed) },
        };
    }

    [UnionCase]
    public static TypeKind Struct(bool isReadOnly)
    {
        return new TypeKind
        {
            Index = 1,
            Data = new BlittableTypeKindData { IsReadOnly = isReadOnly },
        };
    }

    public void Match(Action<bool, bool> classCase, Action<bool> structCase)
    {
        switch (Index)
        {
            case 0:
                classCase(Data.ClassCase.IsAbstract, Data.ClassCase.IsSealed);
                break;
            case 1:
                structCase(Data.IsReadOnly);
                break;
        }
    }

    public TRet Match<TState, TRet>(
        TState state,
        Func<TState, bool, bool, TRet> classCase,
        Func<TState, bool, TRet> structCase
    )
    {
        return Index switch
        {
            0 => classCase(state, Data.ClassCase.IsAbstract, Data.ClassCase.IsSealed),
            1 => structCase(state, Data.IsReadOnly),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
