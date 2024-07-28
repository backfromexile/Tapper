namespace Tapper.Core;

public interface ITypeTranspilerProvider
{
    ITypeDefinitionTranspiler<T> GetTypeDefinitionTranspiler<T>();
    ITypeReferenceTranspiler<T> GetTypeReferenceTranspiler<T>();
}
