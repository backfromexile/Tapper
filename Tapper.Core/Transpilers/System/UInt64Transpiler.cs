namespace Tapper.Core.Transpilers.System;

public class UInt64Transpiler : ITypeReferenceTranspiler<ulong>
{
    public string TranspileTypeReference()
    {
        return "bigint";
    }
}
