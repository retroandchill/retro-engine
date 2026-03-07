// // @file Exceptions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Utils;

namespace RetroEngine.Utils;

[InheritConstructors]
public partial class GraphicsException : Exception;

[InheritConstructors]
public partial class InvalidStateException : Exception;

[InheritConstructors]
public partial class ResourceException : Exception;
