namespace Tapper.Core.Transpilers.System;

public class DateTimeOffsetTranspiler : ITypeReferenceTranspiler<DateTimeOffset>
{
    public string TranspileTypeReference()
    {
        return "Date";
    }
}
