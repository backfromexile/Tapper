namespace Tapper.Core.Transpilers.System;

public class BoolTranspiler : ITypeReferenceTranspiler<bool>
{
    public string TranspileTypeReference()
    {
        return "boolean";
    }
}
