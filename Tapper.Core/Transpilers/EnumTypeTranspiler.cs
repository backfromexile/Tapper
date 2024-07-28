namespace Tapper.Core.Transpilers;

public class EnumTypeTranspiler<T> : ITypeTranspiler<T>
    where T : Enum
{
    public string Transpile()
    {
        throw new NotImplementedException();
    }
}
