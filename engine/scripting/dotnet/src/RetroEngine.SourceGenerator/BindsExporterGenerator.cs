using System.Collections.Immutable;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Types;
using RetroEngine.Binds;
using RetroEngine.SourceGenerator.Model;
using RetroEngine.SourceGenerator.Properties;

namespace RetroEngine.SourceGenerator;

[Generator]
public class BindsExporterGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor ExporterMustBeStatic = new(
        "RE0001",
        "Bind Exporter must be static",
        "{0} must be marked static",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var exportProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                typeof(BindExportAttribute).FullName!,
                (n, _) => n is ClassDeclarationSyntax,
                (ctx, _) =>
                {
                    var type = (TypeDeclarationSyntax)ctx.TargetNode;
                    return ctx.SemanticModel.GetDeclaredSymbol(type) as INamedTypeSymbol;
                })
            .Where(type => type != null);
        
        context.RegisterSourceOutput(exportProvider, ProcessType!);
    }

    private static void ProcessType(SourceProductionContext context, INamedTypeSymbol type)
    {
        var cppNamespace = type.GetBindExportInfo().CppNamespace;
        
        var isValid = true;
        if (!type.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(ExporterMustBeStatic, type.Locations[0], type.Name));
            isValid = false;
        }

        if (!isValid) return;
        
        var bindMethodBuilder = ImmutableArray.CreateBuilder<BindMethodInfo>();
        foreach (var method in type.GetMembers().OfType<IMethodSymbol>()
                     .Where(m => m.DeclaredAccessibility == Accessibility.Public))
        {
            bindMethodBuilder.Add(CreateBindMethodInfo(method));
        }

        var bindMethods = bindMethodBuilder.DrainToImmutable();

        var exporterClassInfo = new ExporterClassInfo
        {
            ManagedNamespace = type.ContainingNamespace.ToDisplayString(),
            CppNamespace = cppNamespace,
            Name = type.Name,
            Methods = bindMethods
        };

        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.RegisterHelper("DelegateSize", (writer, ctx, _) =>
        {
            if (ctx.Value is not BindMethodInfo bindMethodInfo)
            {
                return;
            }
            
            var parameters = new List<string>(bindMethodInfo.Parameters.Length + 1);
            if (!bindMethodInfo.ReturnsVoid)
            {
                parameters.Add(bindMethodInfo.ManagedReturnType);
            }

            parameters.AddRange(bindMethodInfo.Parameters.Select(parameter => parameter.ManagedType));
            
            var fullString = string.Join(" + ", parameters.Select(p => $"sizeof({p})"));
            writer.Write(fullString);
        });
        
        handlebars.RegisterHelper("MethodParameters", (writer, ctx, _) =>
        {
            if (ctx.Value is not BindMethodInfo bindMethodInfo)
            {
                return;
            }
            
            var parameters = string.Join(", ", bindMethodInfo.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.Name}"));
            writer.Write(parameters);
        });
        
        handlebars.RegisterHelper("ParameterNames", (writer, ctx, _) =>
        {
            if (ctx.Value is not BindMethodInfo bindMethodInfo)
            {
                return;
            }
            
            var parameters = string.Join(", ", bindMethodInfo.Parameters.Select(parameter => $"{parameter.Name}"));
            writer.Write(parameters);
        });
        
        var template = handlebars.Compile(SourceTemplates.BindsExporterTemplate);
        context.AddSource($"{type.Name}.generated.h", template(exporterClassInfo));
    }

    private static BindMethodInfo CreateBindMethodInfo(IMethodSymbol method)
    {
        return new BindMethodInfo
        {
            Name = method.Name,
            ManagedReturnType = method.ReturnType.ToDisplayString(),
            CppReturnType = GetCppReturnType(method),
            ReturnsVoid = method.ReturnsVoid,
            Parameters = [..method.Parameters.Select(GetBindMethodParameter)]
        };
    }

    private static string GetCppReturnType(IMethodSymbol method)
    {
        if (method.ReturnsVoid)
        {
            return "void";
        }
        var hasAttr = method.TryGetCppTypeInfoOnReturnValue(out var info);
        var isByRef = method.ReturnsByRef || method.ReturnsByRefReadonly;
        var isReadOnlyByRef = method.ReturnsByRefReadonly;
        
        return InferCppType(
            method.ReturnType,
            isByRef,
            isReadOnlyByRef,
            hasAttr ? info.TypeName : null,
            hasAttr && info.IsConst,
            hasAttr && info.UseReference
        );
    }

    private static BindMethodParameter GetBindMethodParameter(IParameterSymbol parameter)
    {
        return new BindMethodParameter
        {
            Name = parameter.Name,
            ManagedType = GetManagedParameterType(parameter),
            CppType = GetCppParameterType(parameter)
        };
    }

    private static string GetManagedParameterType(IParameterSymbol parameter)
    {
        var baseType = parameter.Type.ToDisplayString();
        return parameter.RefKind switch
        {
            RefKind.RefReadOnlyParameter => $"ref readonly {baseType}",
            RefKind.Ref         => $"ref {baseType}",
            RefKind.Out         => $"out {baseType}",
            RefKind.In         => $"in {baseType}",
            _                   => baseType
        };
    }
    
    private static string GetCppParameterType(IParameterSymbol parameter)
    {
        var hasAttr = false;
        CppTypeInfo info = default;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var attributeData in parameter.GetAttributes())
        {
            if (!attributeData.TryGetCppTypeInfo(out info)) continue;
            hasAttr = true;
            break;
        }

        var refKind = parameter.RefKind;
        var isByRef = refKind is RefKind.Ref or RefKind.Out or RefKind.In or RefKind.RefReadOnly;
        var isReadOnlyByRef = refKind is RefKind.In or RefKind.RefReadOnly;

        return InferCppType(
            parameter.Type,
            isByRef,
            isReadOnlyByRef,
            hasAttr ? info.TypeName : null,
            hasAttr && info.IsConst,
            hasAttr && info.UseReference
        );
    }

    private static string InferCppType(
        ITypeSymbol typeSymbol,
        bool isByRef,
        bool isReadOnlyByRef,
        string? explicitName,
        bool isConstFromAttr,
        bool useReferenceFromAttr)
    {
        var hasExplicitName = !string.IsNullOrWhiteSpace(explicitName);

        // Handle pointer-like first: native pointers and IntPtr
        if (typeSymbol is IPointerTypeSymbol pointerTypeSymbol)
        {
            // Base type name (no *, no &)
            var baseTypeName = hasExplicitName
                ? explicitName!
                : InferCppBaseType(pointerTypeSymbol.PointedAtType);

            return BuildPointerLikeType(
                baseTypeName,
                isConstFromAttr,
                useReferenceFromAttr
            );
        }

        if (typeSymbol.SpecialType == SpecialType.System_IntPtr)
        {
            // IntPtr behaves like void* / const void* by default,
            // but TypeName + UseReference override that.
            var baseTypeName = hasExplicitName ? explicitName! : "void";

            return BuildPointerLikeType(
                baseTypeName,
                isConstFromAttr,
                useReferenceFromAttr
            );
        }

        // Non-pointer types: apply ref / const-ref rules
        var valueTypeName = hasExplicitName
            ? explicitName!
            : InferCppBaseType(typeSymbol);

        if (!isByRef)
        {
            // Passed / returned by value; attribute IsConst & UseReference
            // do NOT change by-value semantics per the rules.
            return valueTypeName;
        }

        // By-ref, non-pointer:
        // - in / ref readonly => const T&
        // - ref / out         => T&
        var constQualifier = isReadOnlyByRef ? "const " : "";
        return $"{constQualifier}{valueTypeName}&";
    }

    private static string BuildPointerLikeType(
        string baseTypeName,
        bool isConstFromAttr,
        bool useReferenceFromAttr)
    {
        // If UseReference is true for a pointer-like type,
        // we turn it into a reference instead of a pointer.
        var indirection = useReferenceFromAttr ? "&" : "*";

        // IsConst on pointer-like => const T* or const T&
        var constQualifier = isConstFromAttr ? "const " : "";

        return $"{constQualifier}{baseTypeName}{indirection}";
    }

    // Base type name, without any ref / pointer decoration
    private static string InferCppBaseType(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Char   => "char16_t",
            SpecialType.System_SByte  => "int8",
            SpecialType.System_Int16  => "int16",
            SpecialType.System_Int32  => "int32",
            SpecialType.System_Int64  => "int64",
            SpecialType.System_Byte   => "uint8",
            SpecialType.System_UInt16 => "uint16",
            SpecialType.System_UInt32 => "uint32",
            SpecialType.System_UInt64 => "uint64",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            // IntPtr is handled in InferCppType as pointer-like.
            _                         => GetCppTypeNameForNonSpecialType(typeSymbol)
        };
    }

    private static string GetCppTypeNameForNonSpecialType(ITypeSymbol typeSymbol)
    {
        var fullName = typeSymbol.ToDisplayString();
        return fullName switch
        {
            "RetroEngine.Core.NativeBool" => "bool",
            "RetroEngine.Strings.Name" => "retro::Name",
            "RetroEngine.Strings.FindName" => "retro::FindType",
            _ => throw new InvalidOperationException($"Cannot infer cpp type for {fullName}")
        };
    }
}