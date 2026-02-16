// // @file MathExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace RetroEngine.Portable.Utils;

public static class MathExtensions
{
    private const float KindaNumber = 1E-4f;
    internal const float SmallNumber = 1E-8f;

    extension(Math)
    {
        public static bool IsNearlyEqual<T>(T a, T b)
            where T : unmanaged, IFloatingPoint<T>
        {
            return Math.IsNearlyEqual(a, b, T.CreateChecked(SmallNumber));
        }

        public static bool IsNearlyEqual<T>(T a, T b, T errorTolerance)
            where T : unmanaged, IFloatingPoint<T>
        {
            return T.Abs(a - b) <= errorTolerance;
        }

        public static T ModF<T>(T x, out T integer)
            where T : unmanaged, IFloatingPoint<T>
        {
            integer = T.Truncate(x);
            var fractionalPart = x - integer;
            return fractionalPart;
        }

        public static (T Integral, T Fractional) ModF<T>(T x)
            where T : unmanaged, IFloatingPoint<T>
        {
            var fractionalPart = Math.ModF(x, out var integralPart);
            return (integralPart, fractionalPart);
        }

        public static T TruncateToHalfIfClose<T>(T f)
            where T : unmanaged, IFloatingPoint<T>
        {
            return Math.TruncateToHalfIfClose(f, T.CreateChecked(KindaNumber));
        }

        public static T TruncateToHalfIfClose<T>(T f, T tolerance)
            where T : unmanaged, IFloatingPoint<T>
        {
            var (integralValue, fractionalValue) = Math.ModF(f);
            var half = T.CreateChecked(f < T.Zero ? -0.5f : 0.5f);
            return integralValue + (Math.IsNearlyEqual(fractionalValue, half, tolerance) ? half : fractionalValue);
        }

        public static T RoundHalfFromZero<T>(T f)
            where T : unmanaged, IFloatingPoint<T>
        {
            var (integralValue, fractionalValue) = Math.ModF(f);
            if (f < T.Zero)
            {
                return fractionalValue > T.CreateChecked(-0.5f) ? integralValue : integralValue - T.One;
            }

            return fractionalValue < T.CreateChecked(0.5f) ? integralValue : integralValue + T.One;
        }

        public static T RoundHalfToZero<T>(T f)
            where T : unmanaged, IFloatingPoint<T>
        {
            var (integralValue, fractionalValue) = Math.ModF(f);
            if (f < T.Zero)
            {
                return fractionalValue < T.CreateChecked(-0.5f) ? integralValue - T.One : integralValue;
            }

            return fractionalValue > T.CreateChecked(0.5f) ? integralValue + T.One : integralValue;
        }
    }
}
