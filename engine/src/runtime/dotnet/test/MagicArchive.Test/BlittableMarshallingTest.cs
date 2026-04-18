// // @file BlittableMarshallingTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Collections.Immutable;
using System.Text;
using MagicArchive.Utilities;

namespace MagicArchive.Test;

public class BlittableMarshallingTest
{
    private static readonly ImmutableArray<(Type Type, bool IsBlittable)> PredefinedTypes =
    [
        (typeof(byte), true),
        (typeof(sbyte), true),
        (typeof(short), true),
        (typeof(ushort), true),
        (typeof(int), true),
        (typeof(uint), true),
        (typeof(long), true),
        (typeof(ulong), true),
        (typeof(float), true),
        (typeof(double), true),
        (typeof(char), true),
        (typeof(Half), true),
        (typeof(Int128), true),
        (typeof(UInt128), true),
        (typeof(Rune), true),
        (typeof(DateTime), true),
        (typeof(bool), false),
        (typeof(decimal), false),
        (typeof(IntPtr), false),
        (typeof(UIntPtr), false),
        (typeof(DateTimeOffset), false),
        (typeof(Guid), false),
    ];

    private static IEnumerable<TestCaseData<bool>> PredefinedTypesData()
    {
        return PredefinedTypes.Select(type => new TestCaseData<bool>(type.IsBlittable)
        {
            TestName = type.Type.Name,
            TypeArgs = [type.Type],
        });
    }

    [Test]
    [TestCaseSource(nameof(PredefinedTypesData))]
    public void PredefinedGenericTypeCheck<T>(bool isBlittable)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<T>(), Is.EqualTo(isBlittable));
            Assert.That(BlittableMarshalling.IsSimpleBlittable<T>(), Is.EqualTo(isBlittable));
        }
    }

    private static IEnumerable<TestCaseData<Type, bool>> NonGenericPredefinedTypesData()
    {
        return PredefinedTypes.Select(type => new TestCaseData<Type, bool>(type.Type, type.IsBlittable)
        {
            TestName = type.Type.Name,
        });
    }

    [Test]
    [TestCaseSource(nameof(NonGenericPredefinedTypesData))]
    public void PredefinedNonGenericTypeCheck(Type type, bool isBlittable)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable(type), Is.EqualTo(isBlittable));
            Assert.That(BlittableMarshalling.IsSimpleBlittable(type), Is.EqualTo(isBlittable));
        }
    }

    [Test]
    public void EnumTypesAreBlittable()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<DayOfWeek>(), Is.True);
            Assert.That(BlittableMarshalling.IsBlittable<FileMode>(), Is.True);
            Assert.That(BlittableMarshalling.IsBlittable<ConsoleColor>(), Is.True);
            Assert.That(BlittableMarshalling.IsBlittable<FileMode>(), Is.True);
            Assert.That(BlittableMarshalling.IsBlittable<TypeCode>(), Is.True);
        }
    }

    [Test]
    public void TupleTypesWithBlittableTypes()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<(int, float)>(), Is.True);
            Assert.That(BlittableMarshalling.IsBlittable<(int, float, DateTimeOffset)>(), Is.False);
            Assert.That(BlittableMarshalling.IsBlittable<(int, float, string)>(), Is.False);
        }
    }

    [Test]
    public void TuplesWithPaddingAreNotBlittable()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<(int, long, long)>(), Is.False);
            Assert.That(BlittableMarshalling.IsBlittable<(byte, int, int)>(), Is.False);
        }
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private readonly record struct SimpleStruct(int Value);

    private readonly record struct ComplexStruct(int Value, float Float);

    private readonly record struct NestedStruct(ComplexStruct Value);

    // ReSharper restore NotAccessedPositionalProperty.Local

    [Test]
    public void UnmanagedStructsWithSingleSimpleBlittableField()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<SimpleStruct>(), Is.True);
            Assert.That(BlittableMarshalling.IsSimpleBlittable<SimpleStruct>(), Is.True);
        }
    }

    [Test]
    public void UnmanagedStructsWithMultipleBlittableFields()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<ComplexStruct>(), Is.False);
            Assert.That(BlittableMarshalling.IsSimpleBlittable<ComplexStruct>(), Is.False);
        }
    }

    [Test]
    public void UnmanagedStructsWithNestedComplexBlittableField()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(BlittableMarshalling.IsBlittable<NestedStruct>(), Is.False);
            Assert.That(BlittableMarshalling.IsSimpleBlittable<NestedStruct>(), Is.False);
        }
    }
}
