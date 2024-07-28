namespace Tapper.Core.Transpilers.System;

public class Int128Transpiler : ITypeReferenceTranspiler<Int128>
{
    public string TranspileTypeReference()
    {
        return "bigint";
    }
}
