namespace MagicArchive.Test.Models;

[Archivable]
public partial class StandardBase
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
}

[Archivable]
public partial class Derived1 : StandardBase
{
    public int DerivedProp1 { get; set; }
    public int DerivedProp2 { get; set; }
}

[Archivable]
public partial class Derived2 : Derived1
{
    public int Derived2Prop1 { get; set; }
    public int Derived2Prop2 { get; set; }
}

[Archivable]
[ArchivableUnion(0, typeof(Impl1))]
[ArchivableUnion(253, typeof(Impl2))]
public partial interface IUnionInterface
{
    int MyProperty { get; }
}

[Archivable]
public partial class Impl1 : IUnionInterface
{
    public int MyProperty { get; set; }
    public long Foo { get; set; }
}

[Archivable]
public partial class Impl2 : IUnionInterface
{
    public int MyProperty { get; set; }
    public string? Bar { get; set; }
}

[Archivable]
[ArchivableUnion(0, typeof(ImplA1))]
[ArchivableUnion(1, typeof(ImplA2))]
public abstract partial class UnionAbstractClass
{
    public virtual int MyProperty { get; set; }
}

[Archivable]
public partial class ImplA1 : UnionAbstractClass
{
    public override int MyProperty { get; set; }
    public long Foo { get; set; }
}

[Archivable]
public partial class ImplA2 : UnionAbstractClass
{
    public override int MyProperty { get; set; }
    public string? Bar { get; set; }
}

[Archivable(GenerateType.NoGenerate)]
public partial interface IForExternalUnion
{
    public int BaseValue { get; set; }
}

[Archivable]
public partial class AForOne : IForExternalUnion
{
    public int BaseValue { get; set; }
    public int MyProperty { get; set; }
}

[Archivable]
public partial class AForTwo : IForExternalUnion
{
    public int BaseValue { get; set; }
    public int MyProperty { get; set; }
}

[ArchivableUnionFormatter(typeof(IForExternalUnion))]
[ArchivableUnion(0, typeof(AForOne))]
[ArchivableUnion(1, typeof(AForTwo))]
public partial class ForExternalUnionFormatter { }

[Archivable(GenerateType.NoGenerate)]
public partial interface IGenericsUnion<T>
{
    public T? NoValue { get; set; }
}

[Archivable]
public partial class BForOne<T> : IGenericsUnion<T>
{
    public T? NoValue { get; set; }
    public int MyProperty { get; set; }
}

[Archivable]
public partial class BForTwo<T> : IGenericsUnion<T>
{
    public T? NoValue { get; set; }
    public int MyProperty { get; set; }
}

[ArchivableUnionFormatter(typeof(IGenericsUnion<>))]
[ArchivableUnion(0, typeof(BForOne<>))]
[ArchivableUnion(1, typeof(BForTwo<>))]
public partial class ForExternalUnionFormatter2<T> { }

[ArchivableUnionFormatter(typeof(IGenericsUnion<string>))]
[ArchivableUnion(0, typeof(BForOne<string>))]
[ArchivableUnion(1, typeof(BForTwo<string>))]
public partial class ForExternalUnionFormatter3 { }

[Archivable]
public partial class NoraType
{
    public IForExternalUnion? ExtUnion { get; set; }
    public UnionAbstractClass? AbstractUnion { get; set; }
}

// Union for record
// https://github.com/Cysharp/MemoryPack/issues/86

[Archivable(SerializeLayout.Explicit)]
public sealed partial record ChargingBookSubmittedEvent(
    [property: ArchiveOrder(1)] string ChargingPlatform,
    [property: ArchiveOrder(2)] decimal Amount
) : AbstractAuditEvent;

[ArchivableUnion(0, typeof(ChargingBookSubmittedEvent))]
[Archivable(SerializeLayout.Explicit)]
public abstract partial record AbstractAuditEvent
{
    [ArchiveOrder(0)]
    public DateTimeOffset EventDate { get; init; }
}
