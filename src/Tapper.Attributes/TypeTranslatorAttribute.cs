using System;

namespace Tapper;

[AttributeUsage(AttributeTargets.Class)]
public class TypeTranslatorAttribute : Attribute
{
    public Type[] Types { get; set; }

    public TypeTranslatorAttribute(params Type[] types)
    {
        Types = types;
    }
}
