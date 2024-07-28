namespace Tapper.Core.Transpilers.System;

public class StringTranspiler : ITypeReferenceTranspiler<string>
{
    public string TranspileTypeReference()
    {
        return "string";
    }
}
