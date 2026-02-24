// // @file UnionCodeGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;
using RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;
using RetroEngine.Portable.SourceGenerator.Unions.Extensions;
using RetroEngine.Portable.Utils;
using TypeInfo = RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing.TypeInfo;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public sealed class UnionCodeGenerator(
    TypeCodeWriter typeCodeWriter,
    IUnionDefinitionGeneratorFactory unionDefinitionGeneratorFactory
) : IUnionCodeGenerator
{
    private static readonly string ThrowIfNullMethod =
        $"{typeof(ExceptionUtils).FullName}.{nameof(ExceptionUtils.ThrowIfNull)}";

    public string Name => "Union";

    public string GenerateCode(UnionInfo unionInfo, INamedTypeSymbol unionTypeSymbol)
    {
        using var codeWriter = CodeWriter.CreateWithDefaultLines();

        CodeWritingUtils.WriteContainingBlocks(
            unionInfo.TypeInfo,
            codeWriter,
            innerBlock =>
            {
                innerBlock.WriteSuppressWarning("CA1000", "For generic unions.");
                var unionDefinitionGenerator = unionDefinitionGeneratorFactory.Create(unionInfo);
                var unionTypeDefinition = new UnionTypeDefinitionGenerator(
                    unionDefinitionGenerator,
                    unionInfo
                ).Generate();
                typeCodeWriter.WriteType(unionTypeDefinition, innerBlock);

                foreach (var typeDefinition in unionDefinitionGenerator.GetAdditionalTypes())
                {
                    typeCodeWriter.WriteType(typeDefinition, innerBlock);
                }
            }
        );

        return codeWriter.ToString();
    }

    private sealed class UnionTypeDefinitionGenerator
    {
        private readonly IUnionDefinitionGenerator _unionDefinitionGenerator;
        private readonly UnionInfo _union;
        private readonly TypeName _nullableUnionTypeName;

        public UnionTypeDefinitionGenerator(IUnionDefinitionGenerator unionDefinitionGenerator, UnionInfo union)
        {
            _unionDefinitionGenerator = unionDefinitionGenerator;
            _union = union;
            _nullableUnionTypeName = new TypeName(_union.TypeInfo, true);
        }

        public TypeDefinition Generate()
        {
            var unionTypeDefinition = new TypeDefinition
            {
                Name = _union.TypeInfo.Name,
                Kind = _unionDefinitionGenerator.TypeKind,
                IsPartial = true,
                InheritedTypes = [TypeInfos.IEquatable(new TypeName(_union.TypeInfo, false)), TypeInfos.IUnion],
                Properties = [.. _union.Cases.Select(GetIsCaseProperty)],
                Methods =
                [
                    .. _union
                        .Cases.Select(GetUnionCaseMethod)
                        .Concat([
                            GetMatchMethod(MatchMethodConfiguration.WithoutStateWithoutReturn),
                            GetMatchMethod(MatchMethodConfiguration.WithoutStateWithReturn),
                            GetMatchMethod(MatchMethodConfiguration.WithStateWithoutReturn),
                            GetMatchMethod(MatchMethodConfiguration.WithStateWithReturn),
                        ])
                        .Concat(_union.Cases.Select(GetTryGetCaseDataMethod))
                        .Concat(GetDefaultEqualsMethods()),
                ],
                Operators = GetEqualityOperators(_nullableUnionTypeName, _unionDefinitionGenerator),
            };

            return _unionDefinitionGenerator.AdjustUnionTypeDefinition(unionTypeDefinition);
        }

        private PropertyDefinition GetIsCaseProperty(UnionCaseInfo unionCase) =>
            new()
            {
                Name = UnionNamesProvider.GetIsCasePropertyName(unionCase.Name),
                Accessibility = Accessibility.Public,
                TypeName = TypeNames.Boolean,
                Getter = new PropertyDefinition.PropertyAccessor(
                    null,
                    PropertyDefinition.PropertyAccessorImpl.Bodied(
                        (_, bodyBlock) =>
                            bodyBlock.AppendLine(
                                $"return {_unionDefinitionGenerator.GetUnionCaseCheckExpression(unionCase)};"
                            )
                    )
                ),
            };

        private MethodDefinition GetUnionCaseMethod(UnionCaseInfo unionCase) =>
            new()
            {
                Name = unionCase.Name,
                ReturnType = new TypeName(_union.TypeInfo, false),
                Accessibility = Accessibility.Public,
                MethodModifier = MethodModifier.Static,
                IsPartial = true,
                Parameters = [.. unionCase.Parameters.Select(x => new MethodParameter(x.TypeName, x.Name))],
                BodyWriter = _unionDefinitionGenerator.GetUnionCaseMethodBodyWriter(unionCase),
            };

        private MethodDefinition GetMatchMethod(MatchMethodConfiguration methodConfiguration)
        {
            var matchDelegateParameters = _union
                .Cases.Select(x => new MethodParameter(
                    methodConfiguration.MatchCaseDelegateTypeProvider(x.Parameters.Select(y => y.TypeName).ToArray()),
                    $"{char.ToLowerInvariant(x.Name[0])}{x.Name.AsSpan(1).ToString()}Case"
                ))
                .ToArray();

            return new MethodDefinition
            {
                Name = "Match",
                ReturnType = methodConfiguration.ReturnType,
                Accessibility = Accessibility.Public,
                Parameters = [.. methodConfiguration.MethodParametersExtender(matchDelegateParameters)],
                GenericParameters = methodConfiguration.GenericParameters,
                BodyWriter = (_, methodBlock) =>
                {
                    foreach (var matchDelegateParameter in matchDelegateParameters)
                    {
                        methodBlock.AppendLine(
                            $"{ThrowIfNullMethod}({matchDelegateParameter.Name}, \"{matchDelegateParameter.Name}\");"
                        );
                    }

                    methodBlock.AppendLine();
                    foreach (
                        var (unionCase, parameterName) in _union.Cases.Zip(
                            matchDelegateParameters,
                            (x, y) => (x, y.Name)
                        )
                    )
                    {
                        methodBlock.AppendLine($"if ({UnionNamesProvider.GetIsCasePropertyName(unionCase.Name)})");
                        using var thenBlock = methodBlock.NewBlock();
                        var argumentsStr = string.Join(
                            ", ",
                            _unionDefinitionGenerator.GetUnionCaseParameterAccessors(unionCase)
                        );
                        thenBlock.AppendLine(methodConfiguration.MatchBodyProvider(parameterName, argumentsStr));
                    }

                    methodBlock.AppendLine(UnionGenerationUtils.ThrowUnionInInvalidStateCode);
                    if (methodConfiguration.ReturnType != TypeNames.Void)
                    {
                        methodBlock.AppendLine("return default!;");
                    }
                },
            };
        }

        private MethodDefinition GetTryGetCaseDataMethod(UnionCaseInfo unionCase) =>
            new()
            {
                Name = UnionNamesProvider.GetTryGetCaseDataMethodName(unionCase.Name),
                ReturnType = TypeNames.Boolean,
                Accessibility = Accessibility.Public,
                Parameters =
                [
                    .. unionCase.Parameters.Select(x => new MethodParameter(
                        x.TypeName,
                        x.Name,
                        MethodParameterModifier.Out
                    )),
                ],
                BodyWriter = (_, methodBodyBlock) =>
                {
                    methodBodyBlock.AppendLine($"if ({UnionNamesProvider.GetIsCasePropertyName(unionCase.Name)})");
                    using (var thenBlock = methodBodyBlock.NewBlock())
                    {
                        foreach (
                            var (parameter, accessor) in unionCase.Parameters.Zip(
                                _unionDefinitionGenerator.GetUnionCaseParameterAccessors(unionCase),
                                (x, y) => (x, y)
                            )
                        )
                        {
                            thenBlock.AppendLine($"{parameter.Name} = {accessor};");
                        }

                        thenBlock.AppendLine("return true;");
                    }

                    foreach (var parameter in unionCase.Parameters)
                    {
                        methodBodyBlock.AppendLine($"{parameter.Name} = default({parameter.TypeName})!;");
                    }

                    methodBodyBlock.AppendLine("return false;");
                },
            };

        private MethodDefinition[] GetDefaultEqualsMethods() =>
            [
                _unionDefinitionGenerator.AdjustSpecificEqualsMethod(
                    new MethodDefinition
                    {
                        Name = "Equals",
                        Accessibility = Accessibility.Public,
                        ReturnType = TypeNames.Boolean,
                        Parameters = [new MethodParameter(_nullableUnionTypeName, "other")],
                    }
                ),
                _unionDefinitionGenerator.AdjustDefaultEqualsMethod(
                    new MethodDefinition
                    {
                        Name = "Equals",
                        Accessibility = Accessibility.Public,
                        ReturnType = TypeNames.Boolean,
                        Parameters = [new MethodParameter(TypeNames.Object(true), "other")],
                        MethodModifier = MethodModifier.Override,
                    }
                ),
                new()
                {
                    Name = "GetHashCode",
                    Accessibility = Accessibility.Public,
                    ReturnType = TypeNames.Int32,
                    MethodModifier = MethodModifier.Override,
                    BodyWriter = _unionDefinitionGenerator.GetGetHashCodeMethodBodyWriter(),
                },
            ];

        private static ImmutableArray<OperatorDefinition> GetEqualityOperators(
            TypeName nullableUnionTypeName,
            IUnionDefinitionGenerator unionDefinitionGenerator
        ) =>
            [
                new()
                {
                    Name = "==",
                    ReturnType = TypeNames.Boolean,
                    Parameters =
                    [
                        new MethodParameter(nullableUnionTypeName, "left"),
                        new MethodParameter(nullableUnionTypeName, "right"),
                    ],
                    BodyWriter = unionDefinitionGenerator.GetEqualityOperatorBodyWriter(),
                },
                new()
                {
                    Name = "!=",
                    ReturnType = TypeNames.Boolean,
                    Parameters =
                    [
                        new MethodParameter(nullableUnionTypeName, "left"),
                        new MethodParameter(nullableUnionTypeName, "right"),
                    ],
                    BodyWriter = static (_, operatorBodyBlock) =>
                        operatorBodyBlock.AppendLine("return !(left == right);"),
                },
            ];
    }

    private sealed class MatchMethodConfiguration(
        TypeName returnType,
        Func<IReadOnlyCollection<TypeName>, TypeName> matchCaseDelegateTypeProvider,
        Func<string, string, string> matchBodyProvider
    )
    {
        private static readonly TypeName StateName = new(
            TypeInfo.SpecialName("TState", TypeInfo.TypeKind.Unknown()),
            false
        );

        private static readonly TypeName RetName = new(
            TypeInfo.SpecialName("TRet", TypeInfo.TypeKind.Unknown()),
            false
        );

        public static readonly MatchMethodConfiguration WithoutStateWithoutReturn = new(
            TypeNames.Void,
            caseTypeParameters => new TypeName(TypeInfos.Action(caseTypeParameters), false),
            (matchHandlerParameterName, caseParameters) => $"{matchHandlerParameterName}({caseParameters}); return;"
        );

        public static readonly MatchMethodConfiguration WithoutStateWithReturn = new(
            RetName,
            caseTypeParameters => new TypeName(TypeInfos.Func(caseTypeParameters, RetName), false),
            (matchHandlerParameterName, caseParameters) => $"return {matchHandlerParameterName}({caseParameters});"
        )
        {
            GenericParameters = [new GenericParameter(RetName.Name, [])],
        };

        public static readonly MatchMethodConfiguration WithStateWithoutReturn = new(
            TypeNames.Void,
            caseTypeParameters => new TypeName(TypeInfos.Action([StateName, .. caseTypeParameters]), false),
            (matchHandlerParameterName, caseParameters) =>
            {
                caseParameters = string.IsNullOrEmpty(caseParameters) ? string.Empty : $", {caseParameters}";
                return $"{matchHandlerParameterName}(state{caseParameters}); return;";
            }
        )
        {
            GenericParameters = [new GenericParameter(StateName.Name, ["allows ref struct"])],
            MethodParametersExtender = methodParameters =>
                methodParameters.Prepend(new MethodParameter(StateName, "state")),
        };

        public static readonly MatchMethodConfiguration WithStateWithReturn = new(
            RetName,
            caseTypeParameters => new TypeName(TypeInfos.Func([StateName, .. caseTypeParameters], RetName), false),
            (matchHandlerParameterName, caseParameters) =>
            {
                caseParameters = string.IsNullOrEmpty(caseParameters) ? string.Empty : $", {caseParameters}";
                return $"return {matchHandlerParameterName}(state{caseParameters});";
            }
        )
        {
            GenericParameters =
            [
                new GenericParameter(StateName.Name, ["allows ref struct"]),
                new GenericParameter(RetName.Name, []),
            ],
            MethodParametersExtender = methodParameters =>
                methodParameters.Prepend(new MethodParameter(StateName, "state")),
        };

        public TypeName ReturnType { get; } = returnType;

        public ImmutableArray<GenericParameter> GenericParameters { get; init; } = [];

        public Func<IReadOnlyCollection<TypeName>, TypeName> MatchCaseDelegateTypeProvider { get; } =
            matchCaseDelegateTypeProvider;

        public Func<string, string, string> MatchBodyProvider { get; } = matchBodyProvider;

        public Func<IEnumerable<MethodParameter>, IEnumerable<MethodParameter>> MethodParametersExtender
        {
            get;
            private init;
        } = x => x;
    }
}
