using System.Collections.Immutable;
using System.Reflection;

namespace Tapper.Core;

internal class TranspilationRootTypesProvider : ITranspilationRootTypesProvider
{
    private readonly ITranspilationRootAssembliesProvider _provider;

    public TranspilationRootTypesProvider(ITranspilationRootAssembliesProvider provider)
    {
        _provider = provider;
    }

    public IReadOnlyList<Type> GetTypes()
    {
        return _provider.Assemblies
            .SelectMany(assembly => GetTranspilationRootTypes(assembly))
            .ToImmutableList();
    }

    private IEnumerable<Type> GetTranspilationRootTypes(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => type.GetCustomAttribute<TranspilationRootAttribute>() is not null);
    }
}
