namespace Tapper.Core.Transpilers.System.Collections.Generic;

internal class ListTranspiler<T> : ITypeReferenceTranspiler<List<T>>,
    ITypeReferenceTranspiler<IList<T>>,
    ITypeReferenceTranspiler<IReadOnlyList<T>>
{
    private readonly ITypeTranspilerProvider _transpilerProvider;

    public ListTranspiler(ITypeTranspilerProvider transpilerProvider)
    {
        _transpilerProvider = transpilerProvider;
    }

    public string TranspileTypeReference()
    {
        var typeRefTranspiler = _transpilerProvider.GetTypeReferenceTranspiler<T>();
        var transpiledElementTypeRef = typeRefTranspiler.TranspileTypeReference();

        return $"{transpiledElementTypeRef}[]";
    }
}
