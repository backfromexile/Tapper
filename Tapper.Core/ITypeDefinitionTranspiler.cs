namespace Tapper.Core;

public interface ITypeDefinitionTranspiler
{
    string TranspileTypeDefinition();
}
public interface ITypeDefinitionTranspiler<T> : ITypeDefinitionTranspiler { }
