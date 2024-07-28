using System.Reflection;
using System.Text;

namespace Tapper.Core.Transpilers;

internal class ObjectTypeTranspiler<T> : ITypeTranspiler<T>
{
    private readonly ITypeTranspilerProvider _typeTranspilerProvider;

    public ObjectTypeTranspiler(ITypeTranspilerProvider typeTranspilerProvider)
    {
        if (typeof(T).IsEnum)
            throw new ArgumentException("Provided type must be an object type");

        _typeTranspilerProvider = typeTranspilerProvider;
    }

    public string Transpile()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //TODO: ignore properties marked with specific ignore attributes like JsonIgnore

        var builder = new StringBuilder();
        builder.AppendLine($"export type {typeof(T)} = {{");

        foreach (var property in properties)
        {
            var propertyTypeTranspiler = _typeTranspilerProvider.GetType()
                .GetMethod(nameof(ITypeTranspilerProvider.GetTypeTranspiler))
                ?.MakeGenericMethod(property.PropertyType)
                ?? throw new InvalidOperationException();

            //TODO invoke

            builder.AppendLine($"{property.Name}:");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }
}
