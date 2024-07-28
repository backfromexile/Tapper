namespace Tapper.Core.Transpilers.System;

public class SingleTranspiler : ITypeReferenceTranspiler<float>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
