using System.Reflection;
using System.Text;

namespace Tapper.Core.Transpilers;

internal class ObjectTypeTranspiler<T> : ITypeDefinitionTranspiler<T>, ITypeReferenceTranspiler<T>
{
    private readonly ITypeTranspilerProvider _typeTranspilerProvider;

    public ObjectTypeTranspiler(ITypeTranspilerProvider typeTranspilerProvider)
    {
        if (typeof(T).IsEnum)
            throw new ArgumentException("Provided type must be an object type");

        _typeTranspilerProvider = typeTranspilerProvider;
    }

    public string TranspileTypeDefinition()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //TODO: ignore properties marked with specific ignore attributes like JsonIgnore

        var builder = new StringBuilder();
        builder.AppendLine($"export type {typeof(T)} = {{");

        foreach (var property in properties)
        {
            string transpiledPropertyTypeReference = GetTranspiledTypeReference(property.PropertyType);

            builder.AppendLine($"{property.Name}: {transpiledPropertyTypeReference}");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private string GetTranspiledTypeReference(Type propertyType)
    {
        var propertyTypeTranspiler = _typeTranspilerProvider
            .GetType()
            .GetMethod(nameof(ITypeTranspilerProvider.GetTypeReferenceTranspiler))
            ?.MakeGenericMethod(propertyType)
            .Invoke(_typeTranspilerProvider, parameters: null)
            as ITypeReferenceTranspiler;

        if (propertyTypeTranspiler is null)
            throw new InvalidOperationException();

        var transpiledPropertyTypeReference = propertyTypeTranspiler.TranspileTypeReference();
        return transpiledPropertyTypeReference;
    }

    public string TranspileTypeReference()
    {
        return typeof(T).Name;
    }
}
