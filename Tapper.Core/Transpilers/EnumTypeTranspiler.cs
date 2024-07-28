namespace Tapper.Core.Transpilers;

public class EnumTypeTranspiler<T> : ITypeDefinitionTranspiler<T>, ITypeReferenceTranspiler<T>
    where T : Enum
{
    public string TranspileTypeDefinition()
    {
        throw new NotImplementedException();
    }

    public string TranspileTypeReference()
    {
        throw new NotImplementedException();
    }
}
