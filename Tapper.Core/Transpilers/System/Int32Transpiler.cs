namespace Tapper.Core.Transpilers.System;

public class Int32Transpiler : ITypeReferenceTranspiler<int>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
