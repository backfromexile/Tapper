namespace Tapper.Core.Transpilers.System;

public class Int64Transpiler : ITypeReferenceTranspiler<long>
{
    public string TranspileTypeReference()
    {
        return "bigint";
    }
}
