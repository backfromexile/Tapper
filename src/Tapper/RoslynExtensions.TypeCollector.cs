using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace Tapper;

public static partial class RoslynExtensions
{
    private static INamedTypeSymbol[]? NamedTypeSymbols;

    /// <summary>
    /// Get NamedTypeSymbols from target project.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(this Compilation compilation)
    {
        if (NamedTypeSymbols is not null)
        {
            return NamedTypeSymbols;
        }

        NamedTypeSymbols = compilation
            .SyntaxTrees
            .SelectMany(syntaxTree =>
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                return syntaxTree.GetRoot()
                    .DescendantNodes()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<INamedTypeSymbol>();
            }).ToArray();

        return NamedTypeSymbols;
    }

    private static INamedTypeSymbol[]? GlobalNamedTypeSymbols;

    /// <summary>
    /// Get NamedTypeSymbols from target project and reference assemblies.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> GetGlobalNamedTypeSymbols(this Compilation compilation)
    {
        if (GlobalNamedTypeSymbols is not null)
        {
            return GlobalNamedTypeSymbols;
        }

        var typeCollector = new GlobalNamedTypeCollector();
        typeCollector.Visit(compilation.GlobalNamespace);

        GlobalNamedTypeSymbols = typeCollector.ToArray();
        return GlobalNamedTypeSymbols;
    }

    private static INamedTypeSymbol[]? TargetTypes;

    public static INamedTypeSymbol[] GetSourceTypes(this Compilation compilation, bool includeReferencedAssemblies)
    {
        if (TargetTypes is not null)
        {
            return TargetTypes;
        }

        var annotationSymbols = compilation.GetTypesByMetadataName("Tapper.TranspilationSourceAttribute");

        var namedTypes = includeReferencedAssemblies ? compilation.GetGlobalNamedTypeSymbols() : compilation.GetNamedTypeSymbols();

        TargetTypes = namedTypes
            .Where(x =>
            {
                var attributes = x.GetAttributes();

                if (attributes.IsEmpty)
                {
                    return false;
                }

                foreach (var attribute in attributes)
                {
                    foreach (var annotationSymbol in annotationSymbols)
                    {
                        if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, annotationSymbol))
                        {
                            return true;
                        }
                    }
                }

                return false;
            })
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToArray();

        return TargetTypes;
    }

    private static IReadOnlyDictionary<INamedTypeSymbol, ITypeTranslator>? CustomTypeTranslators;
    public static IReadOnlyDictionary<INamedTypeSymbol, ITypeTranslator> GetCustomTypeTranslators(this Compilation compilation)
    {
        if (CustomTypeTranslators is not null)
        {
            return CustomTypeTranslators;
        }

        var assembly = Compile(compilation);

        CustomTypeTranslators = assembly.GetTypes()
            .Select(type => (type, attribute: type.GetCustomAttribute<TypeTranslatorAttribute>()))
            .Where(tuple => tuple.attribute is not null)
            .SelectMany(tuple =>
            {
                var targetTypes = Array.ConvertAll(
                    tuple.attribute!.Types,
                    type => compilation.GetTypeByMetadataName(type.FullName ?? throw new InvalidOperationException("Missing type name"))
                        ?? throw new InvalidOperationException($"Failed to load type \"{type.FullName}\""));

                var translator = (ITypeTranslator?)Activator.CreateInstance(tuple.type)
                    ?? throw new InvalidOperationException($"Failed to create an instance of \"{tuple.type.FullName}\"");

                return targetTypes.Select(targetType => (targetType, translator));
            })
            .ToDictionary<(INamedTypeSymbol targetType, ITypeTranslator translator), INamedTypeSymbol, ITypeTranslator>(
                tuple => tuple.targetType,
                tuple => tuple.translator,
                SymbolEqualityComparer.Default);

        return CustomTypeTranslators;
    }

    private static Assembly Compile(Compilation compilation)
    {
        using var memoryStream = new MemoryStream();

        var compilationResult = compilation.Emit(memoryStream);
        if (!compilationResult.Success)
        {
            throw new InvalidOperationException($"Compilation failed with {compilationResult.Diagnostics.Length} errors");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        var assembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);

        return assembly;
    }
}
