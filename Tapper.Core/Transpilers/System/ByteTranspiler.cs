namespace Tapper.Core.Transpilers.System;

public class ByteTranspiler : ITypeReferenceTranspiler<byte>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
