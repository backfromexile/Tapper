namespace Tapper.Core;

public interface ITypeTranspilerProvider
{
    ITypeTranspiler<T> GetTypeTranspiler<T>();
}
