// // @file TypeSymbol.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MagicArchive;

namespace RetroEngine.Scripting.Model;

/// <summary>
/// Base class for all type specifiers in use.
/// </summary>
/// <param name="name">The full name of the type (i.e. Namespace.TypeName).</param>
[Archivable]
[ArchivableUnion(0, typeof(TypeSpecifier))]
[ArchivableUnion(1, typeof(GenericType))]
[JsonDerivedType(typeof(TypeSpecifier))]
[JsonDerivedType(typeof(GenericType))]
public abstract partial class TypeSymbol(string name)
{
    /// <summary>
    /// Full name of the type (ie. Namespace.TypeName).
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Full name of the type as it would appear in code.
    /// In addition to specifying generic arguments, the difference to Name
    /// is that nested classes have a "+" in the backend, while they have a "."
    /// when writing them in code.
    /// </summary>
    [ArchiveIgnore]
    public virtual string FullCodeName => Name.Replace('+', '.');

    /// <summary>
    /// Same as <see cref="FullCodeName"/> but with unbound generic arguments replaced
    /// by blank (eg. List&lt;T&gt; -> List&lt;&gt;). Needed when referring to unbound types in code.
    /// </summary>
    [ArchiveIgnore]
    public virtual string FullCodeNameUnbound => Name.Replace('+', '.');

    /// <summary>
    /// Short name of the type (ie. without namespace).
    /// </summary>
    [ArchiveIgnore]
    public virtual string ShortName => Name;

    /// <inheritdoc />
    public override string ToString() => Name;
}
