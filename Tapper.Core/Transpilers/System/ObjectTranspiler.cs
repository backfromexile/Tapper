namespace Tapper.Core.Transpilers.System;

public class ObjectTranspiler : ITypeReferenceTranspiler<object>
{
    public string TranspileTypeReference()
    {
        return "unknown"; //TODO: check if "any" is better alternative
    }
}
