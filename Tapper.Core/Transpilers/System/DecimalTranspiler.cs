namespace Tapper.Core.Transpilers.System;

public class DecimalTranspiler : ITypeReferenceTranspiler<decimal>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
