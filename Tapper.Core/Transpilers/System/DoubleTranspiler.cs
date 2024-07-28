namespace Tapper.Core.Transpilers.System;

public class DoubleTranspiler : ITypeReferenceTranspiler<double>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
