// @file Vector.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Core.Math;

public interface IVector<TSelf, TValue>
    : IEquatable<TSelf>,
        IEqualityOperators<TSelf, TSelf, bool>,
        IAdditionOperators<TSelf, TSelf, TSelf>,
        ISubtractionOperators<TSelf, TSelf, TSelf>,
        IMultiplyOperators<TSelf, TValue, TSelf>,
        IDivisionOperators<TSelf, TValue, TSelf>
    where TSelf : unmanaged, IVector<TSelf, TValue>
    where TValue : unmanaged
{
    static abstract TSelf Zero { get; }
    static abstract TSelf One { get; }
}

public interface IVector2<TSelf, TValue> : IVector<TSelf, TValue>
    where TSelf : unmanaged, IVector2<TSelf, TValue>
    where TValue : unmanaged
{
    public TValue X { get; }
    public TValue Y { get; }

    static abstract TSelf Create(TValue x, TValue y);

    void Deconstruct(out TValue x, out TValue y);

    static abstract TSelf Right { get; }
    static abstract TSelf Left { get; }
    static abstract TSelf Up { get; }
    static abstract TSelf Down { get; }
}

public interface IVector3<TSelf, TValue> : IVector<TSelf, TValue>
    where TSelf : unmanaged, IVector3<TSelf, TValue>
    where TValue : unmanaged
{
    public TValue X { get; }
    public TValue Y { get; }
    public TValue Z { get; }

    static abstract TSelf Create(TValue x, TValue y, TValue z);
    static abstract TSelf Create<TOther>(TOther other, TValue z)
        where TOther : unmanaged, IVector2<TOther, TValue>;

    void Deconstruct(out TValue x, out TValue y, out TValue z);

    static abstract TSelf Right { get; }
    static abstract TSelf Left { get; }
    static abstract TSelf Up { get; }
    static abstract TSelf Down { get; }
    static abstract TSelf Forward { get; }
    static abstract TSelf Backward { get; }
}

public interface IVector4<TSelf, TValue> : IVector<TSelf, TValue>
    where TSelf : unmanaged, IVector4<TSelf, TValue>
    where TValue : unmanaged
{
    public TValue X { get; }
    public TValue Y { get; }
    public TValue Z { get; }
    public TValue W { get; }

    static abstract TSelf Create(TValue x, TValue y, TValue z, TValue w);
    static abstract TSelf Create<TOther>(TOther other, TValue z, TValue w)
        where TOther : unmanaged, IVector2<TOther, TValue>;
    static abstract TSelf Create<TOther>(TOther other, TValue w)
        where TOther : unmanaged, IVector3<TOther, TValue>;

    void Deconstruct(out TValue x, out TValue y, out TValue z, out TValue w);

    static abstract TSelf Right { get; }
    static abstract TSelf Left { get; }
    static abstract TSelf Up { get; }
    static abstract TSelf Down { get; }
    static abstract TSelf Forward { get; }
    static abstract TSelf Backward { get; }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector2I(int X, int Y) : IVector2<Vector2I, int>
{
    public static Vector2I Create(int x, int y) => new(x, y);

    public static Vector2I Zero => new(0, 0);
    public static Vector2I One => new(1, 1);

    public static Vector2I Right => new(1, 0);
    public static Vector2I Left => new(-1, 0);
    public static Vector2I Up => new(0, -1);
    public static Vector2I Down => new(0, 1);

    public static Vector2I operator +(Vector2I left, Vector2I right)
    {
        return new Vector2I(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2I operator -(Vector2I left, Vector2I right)
    {
        return new Vector2I(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2I operator *(Vector2I left, int right)
    {
        return new Vector2I(left.X * right, left.Y * right);
    }

    public static Vector2I operator /(Vector2I left, int right)
    {
        return new Vector2I(left.X / right, left.Y / right);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector2F(float X, float Y) : IVector2<Vector2F, float>
{
    public static Vector2F Create(float x, float y) => new(x, y);

    public static Vector2F Zero => new(0, 0);
    public static Vector2F One => new(1, 1);
    public static Vector2F Right => new(1, 0);
    public static Vector2F Left => new(-1, 0);
    public static Vector2F Up => new(0, -1);
    public static Vector2F Down => new(0, 1);

    public static Vector2F operator +(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2F operator -(Vector2F left, Vector2F right)
    {
        return new Vector2F(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2F operator *(Vector2F left, float right)
    {
        return new Vector2F(left.X * right, left.Y * right);
    }

    public static Vector2F operator /(Vector2F left, float right)
    {
        return new Vector2F(left.X / right, left.Y / right);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector3I(int X, int Y, int Z) : IVector3<Vector3I, int>
{
    public static Vector3I Create(int x, int y, int z) => new(x, y, z);

    public static Vector3I Create<TOther>(TOther other, int z)
        where TOther : unmanaged, IVector2<TOther, int> => new(other.X, other.Y, z);

    public static Vector3I Zero => new(0, 0, 0);
    public static Vector3I One => new(1, 1, 1);
    public static Vector3I Right => new(1, 0, 0);
    public static Vector3I Left => new(-1, 0, 0);
    public static Vector3I Up => new(0, -1, 0);
    public static Vector3I Down => new(0, 1, 0);
    public static Vector3I Forward => new(0, 0, -1);
    public static Vector3I Backward => new(0, 0, 1);

    public static Vector3I operator +(Vector3I left, Vector3I right)
    {
        return new Vector3I(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3I operator -(Vector3I left, Vector3I right)
    {
        return new Vector3I(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3I operator *(Vector3I left, int right)
    {
        return new Vector3I(left.X * right, left.Y * right, left.Z * right);
    }

    public static Vector3I operator /(Vector3I left, int right)
    {
        return new Vector3I(left.X / right, left.Y / right, left.Z / right);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector3F(float X, float Y, float Z) : IVector3<Vector3F, float>
{
    public static Vector3F Create(float x, float y, float z) => new(x, y, z);

    public static Vector3F Create<TOther>(TOther other, float z)
        where TOther : unmanaged, IVector2<TOther, float> => new(other.X, other.Y, z);

    public static Vector3F Zero => new(0, 0, 0);
    public static Vector3F One => new(1, 1, 1);

    public static Vector3F Right => new(1, 0, 0);
    public static Vector3F Left => new(-1, 0, 0);
    public static Vector3F Up => new(0, -1, 0);
    public static Vector3F Down => new(0, 1, 0);
    public static Vector3F Forward => new(0, 0, -1);
    public static Vector3F Backward => new(0, 0, 1);

    public static Vector3F operator +(Vector3F left, Vector3F right)
    {
        return new Vector3F(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3F operator -(Vector3F left, Vector3F right)
    {
        return new Vector3F(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3F operator *(Vector3F left, float right)
    {
        return new Vector3F(left.X * right, left.Y * right, left.Z * right);
    }

    public static Vector3F operator /(Vector3F left, float right)
    {
        return new Vector3F(left.X / right, left.Y / right, left.Z / right);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector4I(int X, int Y, int Z, int W) : IVector4<Vector4I, int>
{
    public static Vector4I Create(int x, int y, int z, int w) => new(x, y, z, w);

    public static Vector4I Create<TOther>(TOther other, int z, int w)
        where TOther : unmanaged, IVector2<TOther, int> => new(other.X, other.Y, z, w);

    public static Vector4I Create<TOther>(TOther other, int w)
        where TOther : unmanaged, IVector3<TOther, int> => new(other.X, other.Y, other.Z, w);

    public static Vector4I Zero => new(0, 0, 0, 0);
    public static Vector4I One => new(1, 1, 1, 1);

    public static Vector4I Right => new(1, 0, 0, 0);
    public static Vector4I Left => new(-1, 0, 0, 0);
    public static Vector4I Up => new(0, -1, 0, 0);
    public static Vector4I Down => new(0, 1, 0, 0);
    public static Vector4I Forward => new(0, 0, -1, 0);
    public static Vector4I Backward => new(0, 0, 1, 0);

    public static Vector4I operator +(Vector4I left, Vector4I right)
    {
        return new Vector4I(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vector4I operator -(Vector4I left, Vector4I right)
    {
        return new Vector4I(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vector4I operator *(Vector4I left, int right)
    {
        return new Vector4I(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    public static Vector4I operator /(Vector4I left, int right)
    {
        return new Vector4I(left.X / right, left.Y / right, left.Z / right, left.W / right);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector4F(float X, float Y, float Z, float W) : IVector4<Vector4F, float>
{
    public static Vector4F Create(float x, float y, float z, float w) => new(x, y, z, w);

    public static Vector4F Create<TOther>(TOther other, float z, float w)
        where TOther : unmanaged, IVector2<TOther, float> => new(other.X, other.Y, z, w);

    public static Vector4F Create<TOther>(TOther other, float w)
        where TOther : unmanaged, IVector3<TOther, float> => new(other.X, other.Y, other.Z, w);

    public static Vector4F Zero => new(0, 0, 0, 0);
    public static Vector4F One => new(1, 1, 1, 1);
    public static Vector4F Right => new(1, 0, 0, 0);
    public static Vector4F Left => new(-1, 0, 0, 0);
    public static Vector4F Up => new(0, -1, 0, 0);
    public static Vector4F Down => new(0, 1, 0, 0);
    public static Vector4F Forward => new(0, 0, -1, 0);
    public static Vector4F Backward => new(0, 0, 1, 0);

    public static Vector4F operator +(Vector4F left, Vector4F right)
    {
        return new Vector4F(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vector4F operator -(Vector4F left, Vector4F right)
    {
        return new Vector4F(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vector4F operator *(Vector4F left, float right)
    {
        return new Vector4F(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    public static Vector4F operator /(Vector4F left, float right)
    {
        return new Vector4F(left.X / right, left.Y / right, left.Z / right, left.W / right);
    }
}
