namespace Tapper.Core.Transpilers.System;

public class HalfTranspiler : ITypeReferenceTranspiler<Half>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
