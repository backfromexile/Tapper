namespace Tapper.Core.Transpilers.System;

public class UInt16Transpiler : ITypeReferenceTranspiler<ushort>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
