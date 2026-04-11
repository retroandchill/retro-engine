// // @file TypeHelpers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace RetroEngine.Portable.Serialization.Binary.Utilities;

public static class TypeHelpers
{
    public static bool IsAnonymous(Type type)
    {
        return type.Namespace == null
            && type.IsSealed
            && (
                type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal)
                || type.Name.StartsWith("<>__AnonType", StringComparison.Ordinal)
                || type.Name.StartsWith("VB$AnonymousType_", StringComparison.Ordinal)
            )
            && type.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }
}
