namespace Tapper.Core;

internal interface ITranspilationRootTypesProvider
{
    IReadOnlyList<Type> GetTypes();
}
