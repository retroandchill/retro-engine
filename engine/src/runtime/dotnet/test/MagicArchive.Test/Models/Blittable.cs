// // @file Blittable.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace MagicArchive.Test.Models;

[Archivable]
public partial struct BlittableStruct
{
    public int MyProperty1;
    public float MyProperty2;
}

[Archivable]
public partial struct BlittableStruct2
{
    public int MyProperty1 { get; set; }
    public float MyProperty2 { get; set; }
}

[Archivable]
public partial struct BlittableStruct3
{
    public int MyProperty1
    {
        get;
        set => field = Math.Clamp(value, 0, 100);
    }
    public float MyProperty2 { get; set; }
}

[Archivable]
public partial struct BlittableStruct4
{
    public int MyProperty1 { get; set; }

    [ArchiveInclude]
    private float MyProperty2 { get; set; }
}

[Archivable]
public partial struct NonBlittableStruct
{
    public int MyProperty1 { get; set; }
    public float MyProperty2 { get; set; }

    public double Sum
    {
        get => MyProperty1 + MyProperty2;
        set => throw new NotImplementedException();
    }
}

[Archivable]
public partial struct NonBlittableStruct2
{
    public int MyProperty1 { get; set; }
    public double MyProperty2 { get; set; }
}

[Archivable]
public partial struct NonBlittableStruct3
{
    public int MyProperty1 { get; set; }

    [ArchiveIgnore]
    public float MyProperty2 { get; set; }
}

[Archivable]
public partial struct NonBlittableStruct4
{
    public int MyProperty1 { get; set; }

    private float MyProperty2 { get; set; }
}

[Archivable]
[StructLayout(LayoutKind.Explicit)]
public partial struct NonBlittableStruct5
{
    [field: FieldOffset(0)]
    public int MyProperty1 { get; set; }

    [field: FieldOffset(8)]
    public float MyProperty2 { get; set; }
}

[Archivable(SerializeLayout.Explicit)]
public partial struct NonBlittableStruct6
{
    [ArchiveOrder(1)]
    public int MyProperty1 { get; set; }

    [ArchiveOrder(0)]
    public float MyProperty2 { get; set; }
}
