namespace Tapper.Core.Transpilers.System;

public class DateTimeTranspiler : ITypeReferenceTranspiler<DateTime>
{
    public string TranspileTypeReference()
    {
        return "Date";
    }
}
