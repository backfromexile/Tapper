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

    private static IReadOnlyDictionary<INamedTypeSymbol, ITypeConverter>? CustomTypeConverters;
    public static IReadOnlyDictionary<INamedTypeSymbol, ITypeConverter> GetCustomTypeConverters(this Compilation compilation, bool includeReferencedAssemblies, bool includeDefaultConverters)
    {
        if (CustomTypeConverters is not null)
        {
            return CustomTypeConverters;
        }

        var assembly = Compile(compilation);

        var types = CollectTypes(assembly, includeReferencedAssemblies);

        if (includeDefaultConverters)
        {
            var tapperAssembly = typeof(DefaultTypeConverter).Assembly;
            types.AddRange(CollectTypes(tapperAssembly, false));
        }

        CustomTypeConverters = types
            .Select(type => (type, attribute: type.GetCustomAttribute<TypeConverterAttribute>()))
            .Where(tuple => tuple.attribute is not null)
            .SelectMany(tuple =>
            {
                var targetTypes = Array.ConvertAll(
                    tuple.attribute!.Types,
                    type => compilation.GetTypeByMetadataName(type.FullName ?? throw new InvalidOperationException("Missing type name"))
                        ?? throw new InvalidOperationException($"Failed to load type \"{type.FullName}\""));

                var converter = (ITypeConverter?)Activator.CreateInstance(tuple.type)
                    ?? throw new InvalidOperationException($"Failed to create an instance of \"{tuple.type.FullName}\"");

                return targetTypes.Select(targetType => (targetType, converter));
            })
            .ToDictionary<(INamedTypeSymbol targetType, ITypeConverter converter), INamedTypeSymbol, ITypeConverter>(
                tuple => tuple.targetType,
                tuple => tuple.converter,
                SymbolEqualityComparer.Default);

        return CustomTypeConverters;
    }

    private static List<Type> CollectTypes(Assembly rootAssembly, bool includeReferencedAssemblies)
    {
        if (!includeReferencedAssemblies)
        {
            return rootAssembly.GetTypes().ToList();
        }

        var collectedTypes = new List<Type>();

        var visited = new HashSet<AssemblyName>();
        var queue = new Queue<Assembly>();
        queue.Enqueue(rootAssembly);

        while (queue.TryDequeue(out var assembly))
        {
            visited.Add(assembly.GetName());

            var types = assembly.GetTypes();
            collectedTypes.AddRange(types);

            var referencedAssemblyNames = assembly.GetReferencedAssemblies();
            foreach (var referencedAssemblyName in referencedAssemblyNames)
            {
                if (!visited.Contains(referencedAssemblyName))
                {
                    var referencedAssembly = Assembly.Load(referencedAssemblyName);
                    queue.Enqueue(referencedAssembly);
                }
            }
        }

        return collectedTypes;
    }

    private static Assembly? CompiledAssembly;
    private static Assembly Compile(Compilation compilation)
    {
        if (CompiledAssembly is not null)
        {
            return CompiledAssembly;
        }

        using var memoryStream = new MemoryStream();

        var compilationResult = compilation.Emit(memoryStream);
        if (!compilationResult.Success)
        {
            throw new InvalidOperationException($"Compilation failed with {compilationResult.Diagnostics.Length} errors");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        CompiledAssembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);

        return CompiledAssembly;
    }
}
