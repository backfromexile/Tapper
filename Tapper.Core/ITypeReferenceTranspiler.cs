namespace Tapper.Core;

public interface ITypeReferenceTranspiler
{
    string TranspileTypeReference();
}
public interface ITypeReferenceTranspiler<T> : ITypeReferenceTranspiler { }
