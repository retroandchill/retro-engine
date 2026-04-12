namespace MagicArchive.Test.Models;

[Archivable]
public partial class VTWrapper<T>
{
    public T? Versioned { get; set; }
    public int[]? Values { get; set; }
}

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant0 { }

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant1
{
    [ArchiveOrder(0)]
    public int MyProperty1 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant2
{
    [ArchiveOrder(0)]
    public int MyProperty1 { get; set; } = default;

    [ArchiveOrder(1)]
    public long MyProperty2 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant3
{
    [ArchiveOrder(0)]
    public int MyProperty1 { get; set; } = default;

    [ArchiveOrder(1)]
    public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant4
{
    [ArchiveOrder(0)]
    public int MyProperty1 { get; set; } = default;

    //[ArchiveOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class VersionTolerant5
{
    //[ArchiveOrder(0)]
    //public int MyProperty1 { get; set; } = default;

    //[ArchiveOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;

    [ArchiveOrder(5)]
    public ushort[] MyProperty6 { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class Version1
{
    [ArchiveOrder(0)]
    public int Id { get; set; }

    [ArchiveOrder(1)]
    public string Name { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class Version2
{
    [ArchiveOrder(0)]
    public int Id { get; set; }

    //deleted
    //[ArchiveOrder(1)]
    //public string Name { get; set; } = default!;

    [ArchiveOrder(2)]
    public string FirstName { get; set; } = default!;

    [ArchiveOrder(3)]
    public string LastName { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersionTolerant1
{
    [ArchiveOrder(0)]
    public Version MyProperty1 { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersionTolerant2
{
    [ArchiveOrder(0)]
    public Version MyProperty1 { get; set; } = default!;

    [ArchiveOrder(1)]
    public long MyProperty2 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersionTolerant3
{
    [ArchiveOrder(0)]
    public Version MyProperty1 { get; set; } = default!;

    [ArchiveOrder(1)]
    public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersionTolerant4
{
    [ArchiveOrder(0)]
    public Version MyProperty1 { get; set; } = default!;

    //[ArchiveOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersionTolerant5
{
    //[ArchiveOrder(0)]
    //public int MyProperty1 { get; set; } = default;

    //[ArchiveOrder(1)]
    //public long MyProperty2 { get; set; } = default;

    [ArchiveOrder(2)]
    public short MyProperty3 { get; set; } = default;

    [ArchiveOrder(5)]
    public Version MyProperty6 { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersion1
{
    [ArchiveOrder(0)]
    public Version? Id { get; set; }

    [ArchiveOrder(1)]
    public string Name { get; set; } = default!;
}

[Archivable(GenerateType.VersionTolerant)]
public partial class MoreVersion2
{
    [ArchiveOrder(0)]
    public Version? Id { get; set; }

    //deleted
    //[ArchiveOrder(1)]
    //public string Name { get; set; } = default!;

    [ArchiveOrder(2)]
    public string FirstName { get; set; } = default!;

    [ArchiveOrder(3)]
    public string LastName { get; set; } = default!;
}
