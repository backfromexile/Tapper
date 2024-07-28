using System.Reflection;

namespace Tapper.Core;

internal interface ITranspilationRootAssembliesProvider
{
    IReadOnlyList<Assembly> Assemblies { get; }
}
