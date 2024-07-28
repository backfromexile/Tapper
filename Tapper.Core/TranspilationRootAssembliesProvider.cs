using System.Reflection;

namespace Tapper.Core;

internal class TranspilationRootAssembliesProvider : ITranspilationRootAssembliesProvider
{
    public IReadOnlyList<Assembly> Assemblies { get; }

    public TranspilationRootAssembliesProvider(IReadOnlyList<Assembly> assemblies)
    {
        this.Assemblies = assemblies;
    }
}
