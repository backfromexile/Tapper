namespace Tapper.Core.Transpilers.System;

public class Int16Transpiler : ITypeReferenceTranspiler<short>
{
    public string TranspileTypeReference()
    {
        return "number";
    }
}
