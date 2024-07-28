namespace Tapper.Core.Transpilers.System;

public class UInt32Transpiler : ITypeReferenceTranspiler<uint>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
