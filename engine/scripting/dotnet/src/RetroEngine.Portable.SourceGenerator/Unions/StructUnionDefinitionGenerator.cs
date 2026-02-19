// // @file StructUnionDefinitionGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;
using RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;
using RetroEngine.Portable.SourceGenerator.Unions.Extensions;
using TypeInfo = RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing.TypeInfo;
using TypeKind = RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating.TypeKind;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public sealed class StructUnionDefinitionGenerator : IUnionDefinitionGenerator
{
    private readonly UnionInfo _union;
    private readonly UnionImplementationGenerator _unionImplementationGenerator;
    private readonly IReadOnlyDictionary<UnionCaseInfo, IReadOnlyList<CaseParameter>> _casesParameters;

    public TypeKind TypeKind => TypeKind.Struct(false);

    public StructUnionDefinitionGenerator(UnionInfo union)
    {
        _union = union;
        _unionImplementationGenerator = new UnionImplementationGenerator(
            TypeInfo.SpecificType(
                union.TypeInfo.Namespace,
                union.TypeInfo.ContainingType,
                $"{union.Name}BlittableData",
                TypeInfo.TypeKind.ValueType(true, false)
            )
        );
        _casesParameters = union.Cases.ToDictionary(
            unionCase => unionCase,
            IReadOnlyList<CaseParameter> (unionCase) =>
                unionCase
                    .Parameters.Select(caseParameter =>
                        _unionImplementationGenerator.AddCaseParameter(caseParameter, unionCase)
                    )
                    .ToArray()
        );
    }

    public Action<MethodDefinition, CodeWriter> GetUnionCaseMethodBodyWriter(UnionCaseInfo unionCase)
    {
        var unionCaseIndex = GetUnionCaseIndex(unionCase);
        var caseParameters = _casesParameters[unionCase];

        return (def, methodBlock) =>
        {
            methodBlock.Append("return new ").AppendLine(_union.TypeInfo.GetFullyQualifiedName(false));
            using var initializerBlock = methodBlock.NewBlock(';');
            initializerBlock
                .Append("Index = ")
                .Append(unionCaseIndex.ToString(CultureInfo.InvariantCulture))
                .AppendLine(",");

            var root = new InitializerNode();

            foreach (var (methodParameter, caseParameter) in def.Parameters.Zip(caseParameters, (x, y) => (x, y)))
            {
                root.AddAssignment(caseParameter.FieldPath, methodParameter.Name);
            }

            root.WriteTo(initializerBlock);
        };
    }

    public string GetUnionCaseCheckExpression(UnionCaseInfo unionCase)
    {
        var unionCaseIndex = GetUnionCaseIndex(unionCase);
        return $"Index == {unionCaseIndex}";
    }

    public IEnumerable<string> GetUnionCaseParameterAccessors(UnionCaseInfo unionCase)
    {
        var caseParameters = _casesParameters[unionCase];
        return caseParameters.Select(x => x.ValueAccessor("this"));
    }

    public MethodDefinition AdjustDefaultEqualsMethod(MethodDefinition equalsMethod) =>
        equalsMethod with
        {
            BodyWriter = (def, methodBlock) =>
                methodBlock
                    .Append("return ")
                    .Append(def.Parameters[0].Name)
                    .Append(" is ")
                    .Append(_union.TypeInfo)
                    .Append(" && Equals((")
                    .Append(_union.TypeInfo)
                    .Append(")")
                    .Append(def.Parameters[0].Name)
                    .AppendLine(");"),
        };

    public MethodDefinition AdjustSpecificEqualsMethod(MethodDefinition equalsMethod) =>
        equalsMethod with
        {
            BodyWriter = (def, methodBlock) =>
            {
                var otherName = def.Parameters[0].Name;

                methodBlock.Append("if (this.Index != ").Append(otherName).AppendLine(".Index) return false;");

                WriteCasesSwitchBody(
                    methodBlock,
                    (unionCase, caseParameters) =>
                    {
                        var equalityCode = unionCase.HasParameters
                            ? UnionGenerationUtils.GetUnionCaseEqualityCode(
                                caseParameters.Select(x =>
                                    (Type: x.TypeName, x.ValueAccessor("this"), x.ValueAccessor(otherName))
                                )
                            )
                            : "true";
                        return $"return {equalityCode};";
                    }
                );

                methodBlock.AppendLine("return true;");
            },
        };

    public Action<MethodDefinition, CodeWriter> GetGetHashCodeMethodBodyWriter() =>
        (_, methodBlock) =>
        {
            WriteCasesSwitchBody(
                methodBlock,
                (unionCase, caseParameters) =>
                    UnionGenerationUtils.GetUnionCaseHashCodeCode(
                        GetUnionCaseIndex(unionCase),
                        caseParameters.Select(x => (Type: x.TypeName, x.ValueAccessor("this")))
                    )
            );
            methodBlock.AppendLine("return 0;");
        };

    public Action<OperatorDefinition, CodeWriter> GetEqualityOperatorBodyWriter() =>
        (def, operatorBlock) =>
            operatorBlock
                .Append("return ")
                .Append(def.Parameters[0].Name)
                .Append(".Equals(")
                .Append(def.Parameters[1].Name)
                .AppendLine(");");

    public TypeDefinition AdjustUnionTypeDefinition(TypeDefinition typeDefinition)
    {
        typeDefinition = _unionImplementationGenerator.AdjustUnionTypeDefinition(typeDefinition);
        return typeDefinition with
        {
            Attributes = [.. typeDefinition.Attributes, GetLayoutAttribute(LayoutKind.Auto)],
            Properties =
            [
                .. typeDefinition.Properties,
                new PropertyDefinition()
                {
                    Accessibility = Accessibility.Private,
                    TypeName = TypeNames.Byte,
                    Name = "Index",
                    Getter = new PropertyDefinition.PropertyAccessor(),
                    Setter = new PropertyDefinition.PropertyAccessor(),
                    IsInitOnly = true,
                },
            ],
            Methods =
            [
                .. typeDefinition.Methods,
                new MethodDefinition
                {
                    Accessibility = Accessibility.Public,
                    MethodModifier = MethodModifier.Override,
                    ReturnType = TypeNames.String(),
                    Name = "ToString",
                    BodyWriter = (_, methodBlock) =>
                    {
                        WriteCasesSwitchBody(
                            methodBlock,
                            (unionCase, caseParameters) =>
                            {
                                var caseStringRepresentation = UnionGenerationUtils.GetCaseStringRepresentation(
                                    unionCase.Name,
                                    unionCase
                                        .Parameters.Zip(caseParameters, (x, y) => (x.Name, y.ValueAccessor("this")))
                                        .ToArray()
                                );
                                return $"return {caseStringRepresentation};";
                            }
                        );

                        methodBlock
                            .AppendLine(UnionGenerationUtils.ThrowUnionInInvalidStateCode)
                            .AppendLine("return null!;");
                    },
                },
            ],
        };
    }

    public IReadOnlyList<TypeDefinition> GetAdditionalTypes() => _unionImplementationGenerator.GetAdditionalTypes();

    private void WriteCasesSwitchBody(
        CodeWriter codeWriter,
        Func<UnionCaseInfo, IReadOnlyList<CaseParameter>, string> caseStatementProvider
    )
    {
        codeWriter.AppendLine("switch (Index)");
        using var switchBlock = codeWriter.NewBlock();
        foreach (var unionCase in _union.Cases)
        {
            var caseParameters = _casesParameters[unionCase];
            var caseIndex = GetUnionCaseIndex(unionCase);
            switchBlock
                .AppendLine($"case {caseIndex}:")
                .Append("\t")
                .AppendLine(caseStatementProvider(unionCase, caseParameters));
        }
    }

    private int GetUnionCaseIndex(UnionCaseInfo unionCase) => _union.Cases.IndexOf(unionCase) + 1;

    private static string GetLayoutAttribute(LayoutKind layoutKind) =>
        $"global::System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.{layoutKind})";

    private readonly record struct CaseParameter(
        string FieldPath,
        TypeName TypeName,
        Func<string, string> ValueAccessor
    );

    private sealed class InitializerNode
    {
        private readonly Dictionary<string, InitializerNode> _children = new(StringComparer.Ordinal);
        private readonly List<(string Name, string ValueExpression)> _assignments = new();

        public void AddAssignment(string fieldPath, string valueExpression)
        {
            if (string.IsNullOrWhiteSpace(fieldPath))
            {
                return;
            }

            var parts = fieldPath.Split('.');
            if (parts.Length == 0)
            {
                return;
            }

            var node = this;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!node._children.TryGetValue(part, out var next))
                {
                    next = new InitializerNode();
                    node._children.Add(part, next);
                }

                node = next;
            }

            node._assignments.Add((parts[^1], valueExpression));
        }

        public void WriteTo(CodeWriter initializerBlock)
        {
            // Deterministic output for stable generator diffs.
            foreach (var (name, value) in _assignments.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                initializerBlock.Append(name).Append(" = ").Append(value).AppendLine("!,");
            }

            foreach (var (childName, child) in _children.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                initializerBlock.Append(childName).AppendLine(" = new()");
                using var childBlock = initializerBlock.NewBlock(',');
                child.WriteTo(childBlock);
            }
        }
    }

    private sealed class UnionImplementationGenerator(TypeInfo unionBlittableDataTypeInfo)
    {
        private const string BlittableStructFieldName = "UnionBlittableDataField";

        private readonly List<DefaultParameterInfo> _defaultParameters = [];
        private readonly Dictionary<UnionCaseInfo, CaseBlittableStructInfo> _blittableParameters = new();

        public CaseParameter AddCaseParameter(UnionCaseParameterInfo caseParameter, UnionCaseInfo unionCase) =>
            caseParameter.TypeName.TypeInfo.Kind.Match(
                _ =>
                {
                    var referenceTypeParameter = GetOrAddDefaultParameter(TypeNames.Object(), unionCase);
                    return new CaseParameter(
                        referenceTypeParameter.Name,
                        caseParameter.TypeName,
                        v => $"{TypeInfos.Unsafe}.As<{caseParameter.TypeName}>({v}.{referenceTypeParameter.Name})"
                    );
                },
                (isUnmanaged, _) =>
                {
                    if (isUnmanaged && !caseParameter.ContainsGenericParameters)
                    {
                        if (!_blittableParameters.TryGetValue(unionCase, out var blittableStructInfo))
                        {
                            var typeInfo = TypeInfo.SpecificType(
                                unionBlittableDataTypeInfo.Namespace,
                                unionBlittableDataTypeInfo,
                                $"{unionCase.Name}BlittableData",
                                TypeInfo.TypeKind.ValueType(true, false)
                            );
                            blittableStructInfo = new CaseBlittableStructInfo(typeInfo, $"{unionCase.Name}Data", []);
                            _blittableParameters[unionCase] = blittableStructInfo;
                        }

                        blittableStructInfo.Fields.Add(new TypeNamePair(caseParameter.TypeName, caseParameter.Name));
                        var fieldPath =
                            $"{BlittableStructFieldName}.{blittableStructInfo.FieldName}.{caseParameter.Name}";
                        return new CaseParameter(fieldPath, caseParameter.TypeName, v => $"{v}.{fieldPath}");
                    }

                    var defaultParameter = GetOrAddDefaultParameter(caseParameter.TypeName, unionCase);
                    return new CaseParameter(
                        defaultParameter.Name,
                        caseParameter.TypeName,
                        v => $"{v}.{defaultParameter.Name}"
                    );
                },
                () =>
                {
                    var defaultParameter = GetOrAddDefaultParameter(
                        new TypeName(caseParameter.TypeName.TypeInfo, false),
                        unionCase
                    );
                    return new CaseParameter(
                        defaultParameter.Name,
                        caseParameter.TypeName,
                        v => $"{v}.{defaultParameter.Name}"
                    );
                }
            );

        public TypeDefinition AdjustUnionTypeDefinition(TypeDefinition typeDefinition)
        {
            var blittableDataField =
                _blittableParameters.Count > 0
                    ? new PropertyDefinition
                    {
                        Accessibility = Accessibility.Private,
                        IsInitOnly = true,
                        TypeName = new TypeName(unionBlittableDataTypeInfo, false),
                        Name = BlittableStructFieldName,
                        Getter = new PropertyDefinition.PropertyAccessor(),
                        Setter = new PropertyDefinition.PropertyAccessor(),
                    }
                    : null;

            return typeDefinition with
            {
                Properties =
                [
                    .. typeDefinition.Properties,
                    .. _defaultParameters.Select(p => new PropertyDefinition
                    {
                        Accessibility = Accessibility.Private,
                        IsInitOnly = true,
                        TypeName = p.Field.TypeName,
                        Name = p.Field.Name,
                        Getter = new PropertyDefinition.PropertyAccessor(),
                        Setter = new PropertyDefinition.PropertyAccessor(),
                    }),
                    .. blittableDataField != null ? [blittableDataField] : Array.Empty<PropertyDefinition>(),
                ],
            };
        }

        public IReadOnlyList<TypeDefinition> GetAdditionalTypes()
        {
            var blittableDataNestedType = GenerateBlittableDataNestedType();
            return blittableDataNestedType != null ? [blittableDataNestedType] : [];
        }

        private TypeNamePair GetOrAddDefaultParameter(TypeName typeName, UnionCaseInfo unionCase)
        {
            var defaultParameter = _defaultParameters.Find(x =>
                x.Field.TypeName == typeName && !x.UsedBy.Contains(unionCase)
            );
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (defaultParameter.UsedBy is null)
            {
                defaultParameter = new DefaultParameterInfo(
                    new TypeNamePair(typeName, $"Field{_defaultParameters.Count}"),
                    []
                );
                _defaultParameters.Add(defaultParameter);
            }

            defaultParameter.UsedBy.Add(unionCase);
            return defaultParameter.Field;
        }

        private TypeDefinition? GenerateBlittableDataNestedType()
        {
            if (_blittableParameters.Count == 0)
            {
                return null;
            }

            return new TypeDefinition
            {
                Accessibility = Accessibility.Internal,
                Kind = TypeKind.Struct(false),
                Name = unionBlittableDataTypeInfo.Name,
                Attributes = [GetLayoutAttribute(LayoutKind.Explicit)],
                Properties =
                [
                    .. _blittableParameters.Values.Select(x => new PropertyDefinition
                    {
                        Accessibility = Accessibility.Public,
                        IsInitOnly = true,
                        TypeName = new TypeName(x.StructInfo, false),
                        Name = x.FieldName,
                        Attributes = ["field: global::System.Runtime.InteropServices.FieldOffsetAttribute(0)"],
                        Getter = new PropertyDefinition.PropertyAccessor(),
                        Setter = new PropertyDefinition.PropertyAccessor(),
                    }),
                ],
                NestedTypes =
                [
                    .. _blittableParameters.Values.Select(x => new TypeDefinition
                    {
                        Accessibility = Accessibility.Public,
                        Kind = TypeKind.Struct(false),
                        Name = x.StructInfo.Name,
                        Attributes = [GetLayoutAttribute(LayoutKind.Auto)],
                        Properties =
                        [
                            .. x.Fields.Select(y => new PropertyDefinition()
                            {
                                Accessibility = Accessibility.Public,
                                IsInitOnly = true,
                                TypeName = y.TypeName,
                                Name = y.Name,
                                Getter = new PropertyDefinition.PropertyAccessor(),
                                Setter = new PropertyDefinition.PropertyAccessor(),
                            }),
                        ],
                    }),
                ],
            };
        }

        private readonly record struct DefaultParameterInfo(TypeNamePair Field, HashSet<UnionCaseInfo> UsedBy);

        private readonly record struct TypeNamePair(TypeName TypeName, string Name);

        private readonly record struct CaseBlittableStructInfo(
            TypeInfo StructInfo,
            string FieldName,
            List<TypeNamePair> Fields
        );
    }
}
