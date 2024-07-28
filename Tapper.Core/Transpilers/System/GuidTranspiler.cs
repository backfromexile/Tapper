namespace Tapper.Core.Transpilers.System;

public class GuidTranspiler : ITypeReferenceTranspiler<Guid>
{
    public string TranspileTypeReference()
    {
        return "string";
    }
}
