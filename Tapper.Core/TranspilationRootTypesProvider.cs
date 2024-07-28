namespace Tapper.Core;

internal class TranspilationRootTypesProvider : ITranspilationRootTypesProvider
{
    public IReadOnlyList<Type> Types { get; }

    public TranspilationRootTypesProvider(IReadOnlyList<Type> types)
    {
        Types = types;
    }
}
