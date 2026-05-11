// // @file StringTableFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Text;
using Injectio.Attributes;
using RetroEngine.Assets;
using RetroEngine.Portable.Localization.StringTables;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class StringTableFactory(IFileSystem fileSystem) : AssetFactory<StringTable>
{
    public override StringTable CreateAsset(AssetPath path)
    {
        var maxLength = Encoding.UTF8.GetMaxByteCount(Name.MaxRenderedLength);
        Span<char> name = stackalloc char[maxLength];
        var assetNameLength = path.AssetName.WriteUtf16Bytes(name);
        var assetName = name[..assetNameLength];
        var nameWithoutExtension = fileSystem.Path.GetFileNameWithoutExtension(assetName);

        var stringTable = new StringTable { Namespace = nameWithoutExtension };
        StringTableRegistry.Instance.RegisterStringTable(new Name(nameWithoutExtension), stringTable);
        return stringTable;
    }
}
