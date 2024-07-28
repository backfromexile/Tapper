namespace Tapper.Core.Transpilers.System;

public class SByteTranspiler : ITypeReferenceTranspiler<sbyte>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
