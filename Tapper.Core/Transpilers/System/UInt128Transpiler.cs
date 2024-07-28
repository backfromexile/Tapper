namespace Tapper.Core.Transpilers.System;

public class UInt128Transpiler : ITypeReferenceTranspiler<UInt128>
{
    public string TranspileTypeReference()
    {
        return "bigint";
    }
}
