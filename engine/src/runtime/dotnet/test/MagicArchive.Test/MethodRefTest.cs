using System.Buffers;

namespace MagicArchive.Test;

public class MethodRefTest
{
    [Test]
    public void WriteId()
    {
        var data = new EmitIdData { MyProperty = 9999 };
        var bin = ArchiveSerializer.Serialize(data);

        EmitIdData.PrivateData = Guid.Empty;
        var v2 = ArchiveSerializer.Deserialize<EmitIdData>(bin);
        Assert.That(v2, Is.Not.Null);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(v2.MyProperty, Is.EqualTo(data.MyProperty));
        Assert.That(EmitIdData.PrivateData, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void ReadOther()
    {
        var data = new EmitFromOther();
        data.Set(9999);

        var reference = new EmitFromOther();
        EmitFromOther.Other = reference;

        var bin = ArchiveSerializer.Serialize(data);

        var v2 = ArchiveSerializer.Deserialize<EmitFromOther>(bin);
        Assert.That(v2, Is.Not.Null);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(v2.MyProperty, Is.EqualTo(data.MyProperty));
        Assert.That(v2, Is.SameAs(reference));
    }
}

[Archivable]
public partial class EmitIdData
{
    public int MyProperty { get; set; }

    internal static Guid PrivateData;

    [ArchivableOnSerializing]
    private static void WriteId<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, EmitIdData? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteValue(Guid.NewGuid()); // emit GUID in header.
    }

    [ArchivableOnDeserializing]
    private static void ReadId(ref ArchiveReader reader, ref EmitIdData? value)
    {
        // read custom header before deserialize
        var guid = reader.ReadValue<Guid>();
        Console.WriteLine(guid);
        PrivateData = guid;
    }
}

[Archivable]
public partial class EmitFromOther
{
    internal static EmitFromOther Other;

    public int MyProperty { get; private set; }

    public void Set(int v)
    {
        MyProperty = v;
    }

    [ArchivableOnDeserializing]
    private static void ReadId(ref ArchiveReader reader, ref EmitFromOther? value)
    {
        value = Other;
    }
}
