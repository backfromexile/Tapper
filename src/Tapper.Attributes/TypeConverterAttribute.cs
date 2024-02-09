using System;

namespace Tapper;

[AttributeUsage(AttributeTargets.Class)]
public class TypeConverterAttribute : Attribute
{
    public Type[] Types { get; set; }

    public TypeConverterAttribute(params Type[] types)
    {
        Types = types;
    }
}
