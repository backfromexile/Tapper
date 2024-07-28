namespace Tapper.Core.Transpilers.System;

public class CharTranspiler : ITypeReferenceTranspiler<char>
{
    public string TranspileTypeReference()
    {
        return "string";
    }
}
